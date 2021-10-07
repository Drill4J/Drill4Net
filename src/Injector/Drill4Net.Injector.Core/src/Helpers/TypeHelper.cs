using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Helper for getting info about types
    /// </summary>
    internal static class TypeHelper
    {
        private static readonly TypeChecker _typeChecker = new();

        /*************************************************************************************/

        /// <summary>
        /// Get filtered types of assembly by specified options
        /// </summary>
        /// <param name="allTypes"></param>
        /// <param name="flt"></param>
        /// <returns></returns>
        internal static IEnumerable<TypeDefinition> FilterTypes(IEnumerable<TypeDefinition> allTypes, SourceFilterOptions flt)
        {
            var res = new List<TypeDefinition>();
            foreach (var typeDef in allTypes)
            {
                var attrs = typeDef.CustomAttributes.Select(a => a.AttributeType.Name);
                if (!IsTypeNeed(flt, typeDef.FullName, attrs))
                    continue;
                res.Add(typeDef);
            }
            return res;
        }

        internal static bool IsTypeNeed(SourceFilterOptions flt, string typeFullName, IEnumerable<string> attributes)
        {
            (string ns, string typeName) = CommonUtils.GetNamespaceAndTypeName(typeFullName);

            //The <Module> type is a placeholder for declaring classes and methods that do not conform to the CLI model.
            //Normally relevant only in mixed-mode assemblies that contain both code written in a managed language and
            //unmanaged code, such as C or C++. It is empty for pure managed builds. These languages support free functions
            //and global variables. The CLR does not directly support this, methods and variables must always be members of
            //the type. So the metadata generator uses a simple trick, it creates a fake type that will become the home for
            //such functions and variables. The name of this fake type is <Module>. It always has internal accessibility to
            //hide the participants.There is only one of these types, its RID is always 1. The CLR source code calls it a
            //"global class".
            if (typeName == "<Module>")
                return false;
            //
            if (_typeChecker.IsSystemType(typeFullName)) //system's types not needed any way
                return false;
            if (!flt.IsClassNeed(typeFullName))
                return false;
            if (!flt.IsNamespaceNeed(ns))
                return false;

            //attributes
            var not = false;
            foreach (var attr in attributes)
            {
                if (!flt.IsAttributeNeed(attr))
                {
                    not = true;
                    break;
                }
            }
            return !not;
        }

        /// <summary>
        /// Get filtered methods for specified type by options in <paramref name="opts"/>. 
        /// All methods of the nested types will also be found.
        /// </summary>
        /// <param name="typeCtx">Type's context</param>
        /// <param name="opts">Options for method filtering</param>
        /// <returns></returns>
        internal static IEnumerable<MethodDefinition> GetMethods(TypeContext typeCtx, ProbeData opts)
        {
            return GetMethods(typeCtx, null, opts);
        }

        /// <summary>
        /// Get filtered methods for specified type (as root) and the nested type in <paramref name="linkType"/> by options in <paramref name="opts"/>.
        /// This method used for the recurse founding all methods for the <paramref name="rootTypeCtx"/>
        /// </summary>
        /// <param name="rootTypeCtx">Root type's context</param>
        /// <param name="linkType">Current nested type in the <paramref name="rootTypeCtx"/></param>
        /// <param name="opts">Options for the method filtering</param>
        /// <returns></returns>
        internal static IEnumerable<MethodDefinition> GetMethods(TypeContext rootTypeCtx, TypeDefinition linkType, ProbeData opts)
        {
            #region Own methods
            var type = linkType ?? rootTypeCtx.Definition;
            var ownMethods = FilterMethods(type, opts);

            //check for type's characteristics
            var isAsyncStateMachine = IsAsyncMachine(type);
            var isEnumerable = IsEnumerable(type);

            var treeParentClass = rootTypeCtx.InjType;
            var typeFullname = treeParentClass.FullName;
            var asmCtx = rootTypeCtx.AssemblyCtx;

            var methods = new List<MethodDefinition>();
            foreach (var ownMethod in ownMethods)
            {
                #region Check
                if (ownMethod.CustomAttributes.Any(attr => attr.AttributeType.Name == nameof(DebuggerHiddenAttribute)))
                    continue;

                //too small body for special functions (no logic: pure set/get, empty .сtor, etc)
                var source = MethodHelper.CreateMethodSource(ownMethod, isAsyncStateMachine, isEnumerable);
                if (source.MethodType is MethodType.Constructor or MethodType.Setter or MethodType.Getter or
                                         MethodType.EventAdd or MethodType.EventRemove)
                {
                    if (ownMethod.Body.Instructions.Count(a => a.OpCode.Code != Code.Nop) < 6)
                        continue;
                }
                #endregion

                var treeFunc = new InjectedMethod(treeParentClass.AssemblyName, treeParentClass.BusinessType,
                    ownMethod.FullName, source);
                //
                if (!asmCtx.InjMethodByFullname.ContainsKey(treeFunc.FullName))
                    asmCtx.InjMethodByFullname.Add(treeFunc.FullName, treeFunc);
                else { } //strange..
                methods.Add(ownMethod);
                //
                var methodKey = MethodHelper.GetMethodKey(typeFullname, treeFunc.Name);
                if (!asmCtx.InjMethodByKeys.ContainsKey(methodKey))
                    asmCtx.InjMethodByKeys.Add(methodKey, treeFunc);
                else { }

                treeParentClass.Add(treeFunc);
            }
            #endregion
            #region Nested classes
            foreach (var nestedType in type.NestedTypes)
            {
                var realTypeName = TryGetRealTypeName(nestedType);
                var treeType = new InjectedType(nestedType.Module.Name, nestedType.FullName, realTypeName)
                {
                    Source = CreateTypeSource(nestedType),
                    Path = rootTypeCtx.AssemblyCtx.InjAssembly.Path,
                };
                asmCtx.InjClasses.Add(treeType.FullName, treeType);
                treeParentClass.Add(treeType);
                //
                var innerMethods = GetMethods(rootTypeCtx, nestedType, opts);
                methods.AddRange(innerMethods);
            }
            #endregion
            return methods;
        }

        /// <summary>
        /// Does the specified type implement the Enumerable interface?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsEnumerable(TypeDefinition type)
        {
            return type.Interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;
        }

        /// <summary>
        /// Does the specified type implement an asynchronous State Machine?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsAsyncMachine(TypeDefinition type)
        {
            var isAsyncStateMachine =
                type.Methods.FirstOrDefault(a => a.Name == "SetStateMachine") != null ||
                type.Interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
            return isAsyncStateMachine;
        }

        /// <summary>
        /// Filter methods of the type according options
        /// </summary>
        /// <param name="type">The type definition</param>
        /// <param name="probOpts">Options for the filtering</param>
        /// <returns></returns>
        internal static IEnumerable<MethodDefinition> FilterMethods(TypeDefinition type, ProbeData probOpts)
        {
            var isAngleBracket = type.Name.StartsWith("<");
            var fltMethods = type.Methods
                .Where(a => a.HasBody && a.Body.Instructions.Count(a => a.OpCode.Code != Code.Nop) > 3)
                .Where(a => !(isAngleBracket && a.IsConstructor)) //internal compiler's ctor is not needed in any cases
                .Where(a => probOpts.Ctor || (!probOpts.Ctor && !a.IsConstructor)) //may be we skips own ctors
                .Where(a => probOpts.Setter || (!probOpts.Setter && a.Name != "set_Prop" && !a.Name.StartsWith("set_"))) //do we need property setters?
                .Where(a => probOpts.Getter || (!probOpts.Getter && a.Name != "get_Prop" && !a.Name.StartsWith("get_"))) //do we need property getters?
                .Where(a => probOpts.EventAdd || !(a.FullName.Contains("::add_") && !probOpts.EventAdd)) //do we need 'event add'?
                .Where(a => probOpts.EventRemove || !(a.FullName.Contains("::remove_") && !probOpts.EventRemove)) //do we need 'event remove'?
                .Where(a => isAngleBracket || !a.IsPrivate || !(a.IsPrivate && !probOpts.Private)) //do we need business privates?
                .Where(a => !a.FullName.Contains("::get__") && !a.FullName.Contains("::set__")) //for example, inner setters/getters for ASP.NET/Blazor (even in business namespace). Is it needed? Doesn't yet...
                ;
            return fltMethods;
        }

        /// <summary>
        /// Try to get real type name from compiler generated classes. Technically, 
        /// it will be upward search the "normal name" in the type parent's hierarchy.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static string TryGetRealTypeName(TypeDefinition type)
        {
            if (type?.DeclaringType == null)
                return type?.FullName;
            do type = type.DeclaringType;
                while ( 
                        type.DeclaringType != null && 
                        (type.Name.Contains("c__DisplayClass") || type.Name.Contains("<>"))
                      );
            return type?.FullName;
        }

        /// <summary>
        /// Retreive some metadata for type
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        internal static TypeSource CreateTypeSource(TypeDefinition def)
        {
            return new TypeSource
            {
                AccessType = GetAccessType(def),
                IsAbstract = def.IsAbstract,
                IsGeneric = def.IsGenericInstance,
                //IsStatic = ...,
                IsValueType = def.IsValueType,
                IsLocal = def.IsNested
            };
        }

        /// <summary>
        /// Get access type of type
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        internal static AccessType GetAccessType(TypeDefinition def)
        {
            if (def.IsNestedPrivate)
                return AccessType.Private;
            if (def.IsPublic)
                return AccessType.Public;
            return AccessType.Internal;
        }
    }
}

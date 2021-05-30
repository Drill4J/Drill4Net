using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Engine
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
        /// <param name="opts"></param>
        /// <returns></returns>
        internal static IEnumerable<TypeDefinition> FilterTypes(IEnumerable<TypeDefinition> allTypes, SourceFilterOptions opts)
        {
            var res = new List<TypeDefinition>();
            foreach (var typeDef in allTypes)
            {
                var typeName = typeDef.Name;
                if (typeName == "<Module>")
                    continue;
                var typeFullName = typeDef.FullName;
                if (_typeChecker.IsSystemType(typeFullName)) //system's types not needed any way
                    continue;
                if (!opts.IsClassNeed(typeFullName))
                    continue;
                if (!opts.IsNamespaceNeed(typeDef.Namespace))
                    continue;

                //attributes
                var not = false;
                foreach (var attr in typeDef.CustomAttributes)
                {
                    if (!opts.IsNamespaceNeed(typeDef.Namespace))
                    {
                        not = true;
                        break;
                    }
                }
                if (not)
                    continue;
                //
                res.Add(typeDef);
            }
            return res;
        }

        internal static IEnumerable<MethodDefinition> GetMethods(TypeContext typeCtx, InjectorOptions opts)
        {
            return GetMethods(typeCtx, null, opts);
        }

        internal static IEnumerable<MethodDefinition> GetMethods(TypeContext rootTypeCtx, TypeDefinition linkType, InjectorOptions opts)
        {
            #region Own methods
            var type = linkType ?? rootTypeCtx.Definition;
            var ownMethods = FilterMethods(type, opts.Probes);

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
                var methodName = ownMethod.Name;

                //too small body for special functions (no logic: pure set/get, empty .сtor, etc)
                if (methodName.StartsWith("set_") || methodName.StartsWith("get_") || methodName.StartsWith(".ctor") || methodName.StartsWith(".cctor"))
                {
                    if (ownMethod.Body.Instructions.Count(a => a.OpCode.Code != Code.Nop) < 6)
                        continue;
                }

                //check for setter & getter of properties for anonymous types
                //is it useless? But for custom weaving it's very interesting idea...
                if (treeParentClass.IsCompilerGenerated)
                {
                    if (ownMethod.IsSetter && !methodName.StartsWith("set_"))
                        continue;
                    if (ownMethod.IsGetter && !methodName.StartsWith("get_"))
                        continue;
                    if (isAsyncStateMachine && methodName != "MoveNext")
                        continue;
                }
                #endregion

                var source = MethodHelper.CreateMethodSource(ownMethod);
                //Console.WriteLine($"Hash: {methodName} -> {source.HashCode}");
                var treeFunc = new InjectedMethod(treeParentClass.AssemblyName, typeFullname,
                    treeParentClass.BusinessType, ownMethod.FullName, source);
                //
                source.IsAsyncStateMachine = isAsyncStateMachine;
                source.IsMoveNext = methodName == "MoveNext";
                source.IsEnumeratorMoveNext = source.IsMoveNext && isEnumerable;
                source.IsFinalizer = methodName == "Finalize" && ownMethod.IsVirtual;
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

            methods = methods
                .Where(m => m.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType.Name == nameof(DebuggerHiddenAttribute)) == null)
                .ToList();
            return methods;
        }

        internal static bool IsEnumerable(TypeDefinition type)
        {
            return type.Interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;
        }

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
        internal static IEnumerable<MethodDefinition> FilterMethods(TypeDefinition type, ProbesOptions probOpts)
        {
            var isAngleBracket = type.Name.StartsWith("<");
            var fltMethods = type.Methods
                .Where(a => a.HasBody)
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
                IsNested = def.IsNested
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

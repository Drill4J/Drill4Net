using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    internal static class MethodHelper
    {
        private static readonly TypeChecker _typeChecker = new();

        /*************************************************************************************/

        internal static void CorrectMethodBusinessSize(Dictionary<string, InjectedMethod> methods, 
            InjectedMethod caller, string calleeName)
        {
            #region Check
            if (methods == null)
                throw new ArgumentNullException(nameof(methods));
            if (!methods.ContainsKey(calleeName))
                return;
            var callee = methods[calleeName];
            if (!callee.IsCompilerGenerated)
                return;
            #endregion

            //at first, children - callees
            foreach (var subCalleeName in callee.CalleeIndexes.Keys)
            {
                CorrectMethodBusinessSize(methods, callee, subCalleeName);
            }

            //the size of caller consists of own size + all sizes of it's CG callees (already included in them)
            caller.BusinessSize += callee.BusinessSize;
        }

        internal static void MapBusinessFunction(MethodContext ctx)
        {
            var instr = ctx.CurInstruction;
            var treeFunc = ctx.Method;
            var flow = instr.OpCode.FlowControl;
            var code = instr.OpCode.Code;
            var asmCtx = ctx.TypeCtx.AssemblyCtx;

            //calls
            if (instr.Operand is not MethodReference extOp ||
                (flow != FlowControl.Call && code != Code.Ldftn && code != Code.Ldfld))
                return;

            //TODO: cache!
            var extTypeFullName = extOp.DeclaringType.FullName;
            var extTypeName = extOp.DeclaringType.Name;
            var extFullname = extOp.FullName;

            #region Callee's indexes
            //TODO: regex
            var isAngledCtor = extFullname.Contains("/<") && extFullname.Contains("__") && extFullname.EndsWith(".ctor()");
            if (!isAngledCtor)
            {
                const string tokenStart = ":Start<";
                var indStart = extFullname.IndexOf(tokenStart);
                var isAsyncMachineStart = indStart > 0 && extFullname.Contains(".CompilerServices.AsyncTaskMethodBuilder");
                if (isAsyncMachineStart)
                {
                    var ind2 = indStart + tokenStart.Length;
                    var asyncCallee = extFullname.Substring(ind2, extFullname.IndexOf("(") - ind2 - 1);
                    if (asmCtx.InjClasses.ContainsKey(asyncCallee))
                    {
                        var asyncType = asmCtx.InjClasses[asyncCallee];
                        if (asyncType.Filter(typeof(InjectedMethod), false)
                            .FirstOrDefault(a => a.Name == "MoveNext") is InjectedMethod asyncMove)
                        {
                            asyncMove.CGInfo.Caller = treeFunc;
                            treeFunc.CalleeIndexes.Add(asyncMove.FullName, ctx.SourceIndex);
                        }
                    }
                }
                if (!treeFunc.CalleeIndexes.ContainsKey(extFullname) && _typeChecker.CheckByMethodFullName(extFullname))
                    treeFunc.CalleeIndexes.Add(extFullname, ctx.SourceIndex);
            }
            #endregion

            // is compiler generated the external method?
            var extName = extOp.Name;
            if (!extTypeFullName.Contains(">d__") && !extName.StartsWith("<") && !extFullname.Contains("|"))
                return;

            try
            {
                //null is norm for anonymous types
                var extType = asmCtx.InjClasses.ContainsKey(extTypeFullName) ?
                    asmCtx.InjClasses[extTypeFullName] :
                    (asmCtx.InjClasses.ContainsKey(extTypeName) ? asmCtx.InjClasses[extTypeName] : null);

                //extType found, not local func, not 'class-for-all'
                if (!extFullname.Contains("|") && extType?.Name?.EndsWith("/<>c") == false)
                {
                    var extRealMethodName = TryGetBusinessMethod(extFullname, extFullname, true, true);
                    InjectedType realCgType = null;
                    var typeKey = treeFunc.BusinessType;
                    if (asmCtx.InjClasses.ContainsKey(typeKey))
                        realCgType = asmCtx.InjClasses[typeKey];
                    if (realCgType != null && extRealMethodName != null)
                    {
                        var mkey = GetMethodKey(realCgType.FullName, extRealMethodName);
                        if (asmCtx.InjMethodByKeys.ContainsKey(mkey))
                            treeFunc = asmCtx.InjMethodByKeys[mkey];

                        var nestTypes = extType.Filter(typeof(InjectedType), true);
                        foreach (var injectedSimpleEntity in nestTypes)
                        {
                            var nestType = (InjectedType)injectedSimpleEntity;
                            if (nestType.IsCompilerGenerated)
                                nestType.FromMethod = treeFunc.BusinessMethod;
                        }
                    }

                    extType.FromMethod = treeFunc.FullName;
                    //better process all methods, not filter only extName... may be... check it!
                    var extMethods = extType.Filter(typeof(InjectedMethod), true);
                    foreach (var injectedSimpleEntity in extMethods)
                    {
                        var meth = (InjectedMethod)injectedSimpleEntity;
                        if (meth.CGInfo != null)
                            meth.CGInfo.FromMethod = treeFunc.FullName ?? extType.FromMethod;
                        if (meth.Name != extName)
                            continue;
                        if (!treeFunc.CalleeIndexes.ContainsKey(meth.FullName))
                            treeFunc.CalleeIndexes.Add(meth.FullName, ctx.SourceIndex);
                    }
                }
                else
                {
                    if (!asmCtx.InjMethodByFullname.ContainsKey(extFullname))
                        return;
                    var extFunc = asmCtx.InjMethodByFullname[extFullname];
                    if (extFunc.CGInfo != null)
                        extFunc.CGInfo.FromMethod ??= treeFunc.FullName ?? extType?.FromMethod;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Getting real name of func method: [{extOp}]");
            }
        }

        internal static string TryGetBusinessMethod(string typeName, string methodName, bool isCompilerGenerated,
                bool isAsyncStateMachine)
        {
            //TODO: regex!!!
            if (!isCompilerGenerated && !isAsyncStateMachine)
                return null;
            string realMethodName = null;
            try
            {
                if (methodName.Contains("|")) //local funcs
                {
                    var a1 = methodName.Split('>')[0];
                    return a1.Substring(1, a1.Length - 1);
                }
                else
                {
                    var isMoveNext = methodName == "MoveNext";
                    var fromMethodName = typeName.Contains("c__DisplayClass") || typeName.Contains("<>");
                    if (isMoveNext || !fromMethodName && typeName.Contains("/"))
                    {
                        var ar = typeName.Split('/');
                        var el = ar[ar.Length - 1];
                        realMethodName = el.Split('>')[0].Replace("<", null);
                    }
                    else if (fromMethodName)
                    {
                        var tmp = methodName.Replace("<>", null);
                        if (tmp.Contains("<"))
                        {
                            var ar = tmp.Split(' ');
                            realMethodName = ar[ar.Length - 1].Split('<')[1].Split('>')[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(TryGetBusinessMethod));
            }
            return realMethodName;
        }

        internal static MethodType GetMethodType(MethodDefinition def)
        {
            if (def.IsSetter)
                return MethodType.Setter;
            if (def.IsGetter)
                return MethodType.Getter;
            //
            var methodFullName = def.FullName;
            if (methodFullName.Contains("::add_"))
                return MethodType.EventAdd;
            if (methodFullName.Contains("::remove_"))
                return MethodType.EventRemove;
            //
            var type = def.DeclaringType;
            var declAttrs = type.CustomAttributes;
            var compGenAttrName = nameof(CompilerGeneratedAttribute);
            var fullName = def.FullName;
            //the CompilerGeneratedAttribute itself is not enough!
            //not use isMoveNext - this class may be own iterator, not compiler's one
            var isCompilerGeneratedType = /*def.IsPrivate &&*/
                def.Name.StartsWith("<") || fullName.EndsWith(">d::MoveNext()") ||
                fullName.Contains(">b__") || fullName.Contains(">c__") || fullName.Contains(">d__") ||
                fullName.Contains(">f__") || fullName.Contains("|") ||
                declAttrs.FirstOrDefault(a => a.AttributeType.Name == compGenAttrName) != null;
            if (isCompilerGeneratedType)
                return MethodType.CompilerGenerated;
            //
            if (def.IsConstructor)
                return MethodType.Constructor;
            if (methodFullName.EndsWith("::Finalize()"))
                return MethodType.Destructor;
            if (methodFullName.Contains("|"))
                return MethodType.Local;
            //
            return MethodType.Normal;
        }

        internal static bool IsSpecialGeneratedMethod(MethodType type)
        {
            return type is MethodType.EventAdd or MethodType.EventRemove;
        }

        internal static IEnumerable<MethodDefinition> GetMethods(TypeContext typeCtx, InjectorOptions opts)
        {
            return GetMethods(typeCtx, null, opts);
        }

        internal static IEnumerable<MethodDefinition> GetMethods(TypeContext rootTypeCtx, TypeDefinition linkType, InjectorOptions opts)
        {
            #region Own methods
            #region Filter methods
            var probOpts = opts.Probes;
            var type = linkType ?? rootTypeCtx.Definition;
            var isAngleBracket = type.Name.StartsWith("<");
            var ownMethods = type.Methods
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
            #endregion

            //check for type's characteristics

            var interfaces = type.Interfaces;
            var isAsyncStateMachine =
                type.Methods.FirstOrDefault(a => a.Name == "SetStateMachine") != null ||
                interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
            var isEnumerable = interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;

            var treeParentClass = rootTypeCtx.InjType;
            var typeFullname = treeParentClass.FullName;
            var asmCtx = rootTypeCtx.AssemblyCtx;

            var methods = new List<MethodDefinition>();
            foreach (var ownMethod in ownMethods)
            {
                #region Check
                var name = ownMethod.Name;

                //too small body for special functions (no logic: pure set/get, empty .сtor, etc)
                if (name.StartsWith("set_") || name.StartsWith("get_") || name.StartsWith(".ctor") || name.StartsWith(".cctor"))
                {
                    if (ownMethod.Body.Instructions.Count(a => a.OpCode.Code != Code.Nop) < 6)
                        continue;
                }

                //check for setter & getter of properties for anonymous types
                //is it useless? But for custom weaving it's very interesting idea...
                if (treeParentClass.IsCompilerGenerated)
                {
                    if (ownMethod.IsSetter && !name.StartsWith("set_"))
                        continue;
                    if (ownMethod.IsGetter && !name.StartsWith("get_"))
                        continue;
                    if (isAsyncStateMachine && name != "MoveNext")
                        continue;
                }
                #endregion

                var source = CreateMethodSource(ownMethod);
                var treeFunc = new InjectedMethod(treeParentClass.AssemblyName, typeFullname,
                    treeParentClass.BusinessType, ownMethod.FullName, source);
                //
                var methodName = ownMethod.Name;
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
                var methodKey = GetMethodKey(typeFullname, treeFunc.Name);
                if (!asmCtx.InjMethodByKeys.ContainsKey(methodKey))
                    asmCtx.InjMethodByKeys.Add(methodKey, treeFunc);
                else { }

                treeParentClass.Add(treeFunc);
            }
            #endregion
            #region Nested classes
            foreach (var nestedType in type.NestedTypes)
            {
                var realTypeName = TypeHelper.TryGetRealTypeName(nestedType);
                var treeType = new InjectedType(nestedType.Module.Name, nestedType.FullName, realTypeName)
                {
                    Source = TypeHelper.CreateTypeSource(nestedType),
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

        internal static string GetMethodKey(string typeFullname, string methodShortName)
        {
            return $"{typeFullname}::{methodShortName}";
        }

        internal static MethodSource CreateMethodSource(MethodDefinition def)
        {
            return new MethodSource
            {
                AccessType = GetAccessType(def),
                IsAbstract = def.IsAbstract,
                IsGeneric = def.HasGenericParameters,
                IsStatic = def.IsStatic,
                MethodType = GetMethodType(def),
                //IsOverride = ...
                IsNested = def.FullName.Contains("|"),
                HashCode = GetMethodHashCode(def.Body.Instructions),
            };
        }

        internal static AccessType GetAccessType(MethodDefinition def)
        {
            if (def.IsPrivate)
                return AccessType.Private;
            if (def.IsPublic)
                return AccessType.Public;
            return AccessType.Internal;
        }

        internal static string GetMethodHashCode(Mono.Collections.Generic.Collection<Instruction> instructions)
        {
            var s = "";
            foreach (var p in instructions.Where(a => a.OpCode.Code != Code.Nop))
                s += p.ToString();
            return s.GetHashCode().ToString();
        }
    }
}

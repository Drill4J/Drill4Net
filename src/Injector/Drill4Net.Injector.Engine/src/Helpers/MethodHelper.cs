using System;
using System.Linq;
using System.Collections.Generic;
using Serilog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using System.Runtime.CompilerServices;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Helper for getting info about methods
    /// </summary>
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

        /// <summary>
        /// Maps the business function to compiler generated ones.
        /// </summary>
        /// <param name="ctx">The method's context.</param>
        /// <returns></returns>
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
                Log.Error(ex, "Getting real name of func method: [{ExtOp}]", extOp);
            }
        }

        /// <summary>
        /// Tries the get business method from the the string data.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="isCompilerGenerated">if set to <c>true</c> the method is compiler generated.</param>
        /// <param name="isAsyncStateMachine">if set to <c>true</c> the method is located in asynchronous state machine.</param>
        /// <returns></returns>
        internal static string TryGetBusinessMethod(string typeName, string methodName, bool isCompilerGenerated,
                bool isAsyncStateMachine)
        {
            //TODO: regex!!!
            if (!isCompilerGenerated && !isAsyncStateMachine)
                return null;
            string realMethodName = null;
            try
            {
                if (methodName.Contains("|")) //for local funcs
                {
                    var a1 = methodName.Split('>')[0];
                    return a1.Substring(1, a1.Length - 1);
                }
                else
                {
                    var isMoveNext = methodName == "MoveNext";
                    var fromMethodName = !methodName.Contains("::<>n__")
                        && (typeName.Contains("c__DisplayClass") || typeName.Contains("<>"));
                    if (isMoveNext || (!fromMethodName && typeName.Contains("/")))
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

        /// <summary>
        /// Get the type of method by its semantic (ctor, normal, setter/getter, compiler generated, etc)
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
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
            if (IsCompilerGeneratedType(def))
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

        /// <summary>
        /// Method's type is compiler generated
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        internal static bool IsCompilerGeneratedType(MethodDefinition def)
        {
            var fullName = def.FullName;
            var typeAttrs = def.DeclaringType.CustomAttributes;

            //the CompilerGeneratedAttribute itself is not enough!
            //not use isMoveNext - this class may be own iterator, not compiler's one
            var isCompilerGeneratedType = /*def.IsPrivate &&*/
                def.Name.StartsWith("<") || fullName.EndsWith(">d::MoveNext()") ||
                fullName.Contains(">b__") || fullName.Contains(">c__") || fullName.Contains(">d__") ||
                fullName.Contains(">f__") || fullName.Contains("|") ||
                typeAttrs.FirstOrDefault(a => a.AttributeType.Name == nameof(CompilerGeneratedAttribute)) != null;
            return isCompilerGeneratedType;
        }

        /// <summary>
        /// Is the method special even for compiler generated ones? Currently it's for the adding and removing events
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsSpecialGeneratedMethod(MethodType type)
        {
            return type is MethodType.EventAdd or MethodType.EventRemove;
        }

        internal static string GetMethodKey(string typeFullname, string methodShortName)
        {
            return $"{typeFullname}::{methodShortName}";
        }

        internal static MethodSource CreateMethodSource(MethodDefinition def, bool isAsyncStateMachine, bool isEnumerableType)
        {
            var methodName = def.Name;
            var isMoveNext = methodName == "MoveNext";
            return new MethodSource
            {
                AccessType = GetAccessType(def),
                IsAbstract = def.IsAbstract,
                IsGeneric = def.HasGenericParameters,
                IsStatic = def.IsStatic,
                MethodType = GetMethodType(def),
                IsAsyncStateMachine = isAsyncStateMachine,
                IsMoveNext = isMoveNext,
                IsEnumeratorMoveNext = isMoveNext && isEnumerableType,
                IsFinalizer = methodName == "Finalize" && def.IsVirtual,
                //IsOverride = ...
                IsLocal = def.FullName.Contains("|"),
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
            var bizInstrs = instructions.Where(a => a.OpCode.Code != Code.Nop);
            unchecked // Overflow is fine, just wrap
            {
                var hash = bizInstrs.Aggregate(3151131, (val, p) => 
                    val ^ $"{p.OpCode.Code}{p.Operand}"
                           .Aggregate(7171371, (strSum, ch) => strSum ^ ch.GetHashCode()));
                return hash.ToString();
            }
        }
    }
}

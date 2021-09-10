using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Helper for working with some contexts
    /// </summary>
    public static class ContextHelper
    {
        /// <summary>
        /// Prepare the Run's and Assembly's contexts
        /// </summary>
        /// <param name="runCtx">Context of Injector Engine's Run</param>
        /// <param name="asmCtx">Context of current assembly</param>
        /// <returns>Does the context contain any methods of interest to us and is it worth taking it into account?</returns>
        public static bool PrepareContextData(RunContext runCtx, AssemblyContext asmCtx)
        {
            var treeAsm = asmCtx.InjAssembly;
            var opts = runCtx.Options;
            var types = TypeHelper.FilterTypes(asmCtx.Module.Types, opts.Source.Filter);

            foreach (var typeDef in types)
            {
                //tree
                var typeFullName = typeDef.FullName;
                var realTypeName = TypeHelper.TryGetRealTypeName(typeDef);
                var treeMethodType = new InjectedType(treeAsm.Name, typeFullName, realTypeName)
                {
                    Source = TypeHelper.CreateTypeSource(typeDef),
                    Path = treeAsm.Path,
                };
                var typeCtx = new TypeContext(asmCtx, typeDef, treeMethodType);

                //collect methods including business & compiler's nested classes
                //together (for async, delegates, anonymous types...)
                var methods = TypeHelper.GetMethods(typeCtx, opts.Probes).ToArray();
                if (!methods.Any())
                    continue;

                asmCtx.TypeContexts.Add(typeFullName, typeCtx);
                asmCtx.InjClasses.Add(treeMethodType.FullName, treeMethodType);
                treeAsm.Add(treeMethodType);

                //by methods
                foreach (var methodDef in methods)
                {
                    var methodFullName = methodDef.FullName;
                    var treeFunc = asmCtx.InjMethodByFullname[methodFullName];

                    var methodCtx = new MethodContext(typeCtx, treeFunc, methodDef);
                    methodCtx.IsStrictEdgeCrosspoints = IsEnterReturnRestrict(runCtx, methodCtx);
                    methodCtx.StartIndex = CalcStartIndex(treeFunc.Source, methodCtx.Definition.Body);
                    typeCtx.MethodContexts.Add(methodFullName, methodCtx);
                }
            }
            if (!asmCtx.TypeContexts.Any())
                return false;

            PrepareProxyCalls(asmCtx, opts);
            return true;
        }

        /// <summary>
        /// Do we need restrict Enter and Return cross-point's injection?
        /// </summary>
        /// <param name="runCtx">Injector Engine's Run context</param>
        /// <param name="methCtx">Method's context</param>
        /// <returns></returns>
        public static bool IsEnterReturnRestrict(RunContext runCtx, MethodContext methCtx)
        {
            var methodName = methCtx.Definition.Name;
            var treeFunc = methCtx.Method;
            var methodSource = treeFunc.Source;
            var methodType = methodSource.MethodType;
            var isCompilerGenerated = methodType == MethodType.CompilerGenerated;
            var isAsyncStateMachine = methodSource.IsAsyncStateMachine;
            var isSpecFunc = MethodHelper.IsSpecialGeneratedMethod(methodType);
            var strictEnterReturn = //is point forbidden principally?
                !isSpecFunc
                //ASP.NET & Blazor rendering methods (may contains business logic)
                && !methodName.Contains("CreateHostBuilder")
                && !methodName.Contains("BuildRenderTree")
                //others
                && (
                    methodName.Contains('|') || //local func                                                        
                    isAsyncStateMachine || //async/await
                    isCompilerGenerated ||
                    //Finalize() -> strange, but for Core 'Enter' & 'Return' lead to a crash                   
                    (runCtx.IsNetCore == true && methodSource.IsFinalizer)
                );
            return strictEnterReturn;
        }

        /// <summary>
        /// Calculate the start index (we need skip the start of instruction array, for example,
        /// for some compiler generated methods)
        /// </summary>
        /// <param name="methodSource"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static int CalcStartIndex(MethodSource methodSource, MethodBody body)
        {
            var methodType = methodSource.MethodType;
            var isCompilerGenerated = methodType == MethodType.CompilerGenerated;
            var isAsyncStateMachine = methodSource.IsAsyncStateMachine;
            var skipStart = isAsyncStateMachine || methodSource.IsEnumeratorMoveNext; //skip the init jump block for the state machine, etc
            var instructions = body.Instructions; //no copy list!

            var startInd = 0;
            if (skipStart)
            {
                //we need find the start of the business part of the code
                //the MoveNext method of Async Machine consists of some own if/else & try/catch statements
                if (isAsyncStateMachine && body.ExceptionHandlers.Any())
                {
                    var minOffset = body.ExceptionHandlers.Min(a => a.TryStart.Offset);
                    var asyncInstr = body.ExceptionHandlers
                        .First(a => a.TryStart.Offset == minOffset).TryStart;
                    startInd = instructions.IndexOf(asyncInstr) + 1;
                    while (true)
                    {
                        var curAsyncCode = asyncInstr.OpCode.Code;
                        //the async's if/else statements are over
                        if (curAsyncCode is Code.Nop or Code.Stfld or Code.Newobj or Code.Call //guanito
                            || curAsyncCode.ToString().StartsWith("Ldarg"))
                            break;
                        asyncInstr = asyncInstr.Next;
                        startInd++;
                    }
                }
                else
                {
                    startInd = 12;
                }
            }
            return startInd;
        }

        /// <summary>
        /// Prepare info about injector proxy class and its called method
        /// </summary>
        /// <param name="asmCtx"></param>
        /// <param name="opts"></param>
        public static void PrepareProxyCalls(AssemblyContext asmCtx, InjectorOptions opts)
        {
            asmCtx.ProxyNamespace = ProxyHelper.CreateProxyNamespace();
            asmCtx.ProxyMethRef = ProxyHelper.CreateProxyMethodReference(asmCtx, opts);
        }
    }
}

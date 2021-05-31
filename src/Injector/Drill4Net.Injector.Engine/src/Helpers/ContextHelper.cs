using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Helper for working with some contexts
    /// </summary>
    internal static class ContextHelper
    {
        /// <summary>
        /// Prepare the Run's and Assembly's contexts
        /// </summary>
        /// <param name="runCtx">Context of Injector Engine's Run</param>
        /// <param name="asmCtx">Context of current assembly</param>
        internal static bool PrepareContextData(RunContext runCtx, AssemblyContext asmCtx)
        {
            var treeAsm = asmCtx.InjAssembly;
            var opts = runCtx.Options;
            var types = TypeHelper.FilterTypes(asmCtx.Module.Types, opts.Source.Filter);

            foreach (var typeDef in types)
            {
                var typeFullName = typeDef.FullName;

                //tree
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
                    #region Init
                    var methodName = methodDef.Name;
                    var methodFullName = methodDef.FullName;

                    //Tree
                    var treeFunc = asmCtx.InjMethodByFullname[methodFullName];
                    var methodSource = treeFunc.Source;
                    var methodType = methodSource.MethodType;

                    var isCompilerGenerated = methodType == MethodType.CompilerGenerated;
                    var isAsyncStateMachine = methodSource.IsAsyncStateMachine;
                    var skipStart = isAsyncStateMachine || methodSource.IsEnumeratorMoveNext; //skip the init jump block for the state machine, etc

                    //Enter/Return
                    var isSpecFunc = MethodHelper.IsSpecialGeneratedMethod(methodType);
                    var strictEnterReturn = //what is principally forbidden
                        !isSpecFunc
                        //ASP.NET & Blazor rendering methods (may contains business logic)
                        && !methodName.Contains("CreateHostBuilder")
                        && !methodName.Contains("BuildRenderTree")
                        //others
                        && (
                            methodName.Contains("|") || //local func                                                        
                            isAsyncStateMachine || //async/await
                            isCompilerGenerated ||
                            //Finalize() -> strange, but for Core 'Enter' & 'Return' lead to a crash                   
                            (runCtx.IsNetCore == true && methodSource.IsFinalizer)
                        );

                    //instructions
                    var body = methodDef.Body;
                    var instructions = body.Instructions; //no copy list!
                    var processor = body.GetILProcessor();
                    #endregion
                    #region Start index
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
                    #endregion
                    #region Method context
                    var methodCtx = new MethodContext(typeCtx, treeFunc, methodDef)
                    {
                        StartIndex = startInd,
                        IsStrictEnterReturn = strictEnterReturn,
                    };
                    typeCtx.MethodContexts.Add(methodFullName, methodCtx);
                    #endregion
                }
            }
            if (!asmCtx.TypeContexts.Any())
                return false;

            //get the injecting commands
            asmCtx.ProxyNamespace = ProxyHelper.CreateProxyNamespace();
            asmCtx.ProxyMethRef = ProxyHelper.CreateProxyMethodReference(asmCtx, opts);

            return true;
        }
    }
}

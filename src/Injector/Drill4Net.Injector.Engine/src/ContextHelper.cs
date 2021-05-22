using System.IO;
using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    internal static class ContextHelper
    {
        internal static bool CreateContexts(RunContext runCtx, AssemblyContext asmCtx)
        {
            var sourceDir = asmCtx.SourceDir;
            var destDir = asmCtx.DestinationDir;
            var asmFullName = asmCtx.Definition.FullName;
            var tree = runCtx.Tree;

            //directory
            var treeDir = tree.GetDirectory(sourceDir);
            if (treeDir == null)
            {
                treeDir = new InjectedDirectory(sourceDir, destDir);
                tree.Add(treeDir);
            }

            //assembly (exactly from whole tree, not just current treeDir - for shared dll)
            var treeAsm = tree.GetAssembly(asmFullName, true) ??
                          new InjectedAssembly(asmCtx.Version, asmCtx.Module.Name, asmFullName, runCtx.SourceFile);
            treeDir.Add(treeAsm);
            asmCtx.InjAssembly = treeAsm;

            var key = asmCtx.Key;
            var keys = runCtx.AssemblyKeys;
            if (keys.ContainsKey(key)) //the assembly is shared and already is injected
            {
                var writer = new AssemblyWriter();
                var copyFrom = keys[key];
                var copyTo = writer.GetDestFileName(copyFrom, destDir);
                File.Copy(copyFrom, copyTo, true);
                return false;
            }
            return true;
        }

        internal static void PrepareContextData(RunContext runCtx, AssemblyContext asmCtx)
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
                var methods = MethodHelper.GetMethods(typeCtx, opts).ToArray();
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
                    var skipStart = isAsyncStateMachine || methodSource.IsEnumeratorMoveNext; //skip state machine init jump block, etc

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
                        if (isAsyncStateMachine && body.ExceptionHandlers.Any())
                        {
                            var minOffset = body.ExceptionHandlers.Min(a => a.TryStart.Offset);
                            var asyncInstr = body.ExceptionHandlers
                                .First(a => a.TryStart.Offset == minOffset).TryStart;
                            startInd = instructions.IndexOf(asyncInstr) + 1;
                            while (true)
                            {
                                var curAsyncCode = asyncInstr.OpCode.Code;
                                //guanito
                                if (curAsyncCode is Code.Nop or Code.Stfld or Code.Newobj or Code.Call
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
                return;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Serilog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Drill4Net.Common;
using Drill4Net.Injection;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Strategies.Flow;

namespace Drill4Net.Injector.Engine
{
    public class InjectorEngine : IInjectorEngine
    {
        /* INFO *
            http://ilgenerator.apphb.com/ - online C# -> IL
            https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C# (buggy and with restrictions!)
                on Github - https://github.com/adrianoc/cecilifier
            https://www.codeproject.com/Articles/671259/Reweaving-IL-code-with-Mono-Cecil
            https://blog.elishalom.com/2012/02/04/monitoring-execution-using-mono-cecil/
            https://stackoverflow.com/questions/48090703/run-mono-cecil-in-net-core
        */

        private readonly IInjectorRepository _rep;
        private readonly InstructionHandlerStrategy _strategy;
        private readonly TypeChecker _typeChecker;

        /***************************************************************************************/

        public InjectorEngine(IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _typeChecker = new TypeChecker();

            FlowStrategy flowStrategy = new(rep.Options.Probes);
            _strategy = flowStrategy;
        }

        /***************************************************************************************/

        public InjectedSolution Process()
        {
            return Process(_rep.Options);
        }

        public InjectedSolution Process(InjectorOptions opts)
        {
            Log.Information("Process starting...");
            InjectorOptionsHelper.ValidateOptions(opts);

            var sourceDir = opts.Source.Directory;
            var destDir = opts.Destination.Directory;

            //copying of all needed data in needed targets
            var monikers = opts.Versions?.Targets;
            _rep.CopySource(sourceDir, destDir, monikers); //TODO: copy dirs only according to the filter
            
            //tree
            var tree = new InjectedSolution(opts.Target?.Name, sourceDir)
            {
                StartTime = DateTime.Now,
                DestinationPath = destDir,
            };

            //ctx of this Run
            var runCtx = new RunContext(_rep, tree);

            //targets from in cfg
            var dirs = Directory.GetDirectories(sourceDir, "*");
            foreach (var dir in dirs)
            {
                //filter by cfg
                if (!opts.Source.Filter.IsDirectoryNeed(dir))
                    continue;

                //filter by target moniker (typed version)
                var need = monikers == null || monikers.Count == 0 || monikers.Any(a =>
                {
                    var x = Path.Combine(sourceDir, a.Value.BaseFolder);
                    if (x.EndsWith("\\"))
                        x = x.Substring(0, x.Length - 1);
                    var z = Path.Combine(dir, a.Key);
                    return x == z;
                });
                
                if (need)
                {
                    runCtx.SourceDirectory = dir;
                    ProcessDirectory(runCtx);
                }
            }

            //the tree's deploying
            tree.RemoveEmpties();
            var deployer = new TreeDeployer(runCtx.Repository);
            deployer.InjectTree(tree); //copying tree data to target root directories
            tree.FinishTime = DateTime.Now;

            #region Debug
            // debug TODO: to tests
            //var methods = tree.GetAllMethods().ToList();
            //var cgMeths = methods.Where(a => a.IsCompilerGenerated).ToList();
            //var emptyCGInfoMeths = cgMeths
            //    .Where(a => a.CGInfo == null)
            //    .ToList();
            //var emptyBusinessMeths = cgMeths
            //    .Where(a => a.CGInfo!= null && a.CGInfo.Caller != null && (a.BusinessMethod == null || a.BusinessMethod == a.FullName))
            //    .ToList();
            //var nonBlockings = cgMeths.FirstOrDefault(a => a.FullName == "System.String Drill4Net.Target.Common.InjectTarget/<>c::<Async_Linq_NonBlocking>b__54_0(Drill4Net.Target.Common.GenStr)");
            //
            //var points = tree.GetAllPoints().ToList();
            #endregion
            return tree;
        }

        internal bool ProcessDirectory(RunContext runCtx)
        {
            var opts = runCtx.Options;
            var directory = runCtx.SourceDirectory;
            if (!opts.Source.Filter.IsDirectoryNeed(directory))
                return false;
            var folder = new DirectoryInfo(directory).Name;
            if (!opts.Source.Filter.IsFolderNeed(folder))
                return false;
            Log.Debug($"Processing dir [{directory}]");

            //files
            var files = _rep.GetAssemblies(directory);
            foreach (var file in files)
            {
                runCtx.SourceFile = file;
                ProcessFile(runCtx);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*");
            foreach (var dir in dirs)
            {
                runCtx.SourceDirectory = dir;
                ProcessDirectory(runCtx);
            }
            return true;
        }
        
        private void ProcessFile(RunContext runCtx)
        {
            #region Checks
            var opts = runCtx.Options;
            var filePath = runCtx.SourceFile;

            //filter
            if (!opts.Source.Filter.IsFileNeed(filePath))
                return;
            if (!_typeChecker.CheckByAssemblyPath(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");
            #endregion
            #region Reading
            var reader = new AssemblyReader();
            var writer = new AssemblyWriter();
            var asmCtx = reader.ReadAssembly(runCtx);
            if (asmCtx.Skipped)
                return;
            #endregion
            #region Processing
            if (!CreateContexts(runCtx, asmCtx))
                return; //it's norm (the assembly is shared and already is injected)

            //get the injecting commands
            var proxyNamespace = CreateProxyNamespace();
            var proxyMethRef = CreateProxyMethodReference(asmCtx, proxyNamespace, opts);

            //preparing data
            PrepareContextData(runCtx, asmCtx, proxyNamespace, proxyMethRef);

            AssemblyHelper.FindMoveNextMethods(asmCtx);
            AssemblyHelper.MapBusinessMethodFirstPass(asmCtx);
            AssemblyHelper.MapBusinessMethodSecondPass(asmCtx);
            AssemblyHelper.CalcBusinessPartCodeSizes(asmCtx);

            //the injecting here
            InjectProxyCalls(asmCtx, runCtx.Tree);
            InjectProxyClass(asmCtx, proxyNamespace, opts);

            //coverage data
            CoverageHelper.CalcCoverageBlocks(asmCtx);
            #endregion

            //save modified assembly and symbols to new file    
            var modifiedPath = writer.SaveAssembly(runCtx, asmCtx);
            asmCtx.Definition.Dispose();

            Log.Information($"Modified assembly is created: {modifiedPath}");
        }

        internal bool CreateContexts(RunContext runCtx, AssemblyContext asmCtx)
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

            var paths = runCtx.Paths;
            if (paths.ContainsKey(asmFullName)) //the assembly is shared and already is injected
            {
                var writer = new AssemblyWriter();
                var copyFrom = paths[asmFullName];
                var copyTo = writer.GetDestFileName(copyFrom, destDir);
                File.Copy(copyFrom, copyTo, true);
                return false;
            }
            return true;
        }

        internal string CreateProxyNamespace()
        {
            //must be unique for each target asm
            return $"Injection_{Guid.NewGuid()}".Replace("-", null); 
        }

        internal MethodReference CreateProxyMethodReference(AssemblyContext asmCtx, string proxyNamespace, InjectorOptions opts)
        {
            //we will use proxy class (with cached Reflection) leading to real profiler
            //proxy will be inject in each target assembly - let construct the calling of it's method
            var module = asmCtx.Module;
            var proxyReturnTypeRef = module.TypeSystem.Void;
            var proxyTypeRef = new TypeReference(proxyNamespace, opts.Proxy.Class, module, module);
            var proxyMethRef = new MethodReference(opts.Proxy.Method, proxyReturnTypeRef, proxyTypeRef);
            var strPar = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            proxyMethRef.Parameters.Add(strPar);
            return proxyMethRef;
        }

        internal void PrepareContextData(RunContext runCtx, AssemblyContext asmCtx, string proxyNamespace, MethodReference proxyMethRef)
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
                var methods = MethodHelper.GetMethods(typeCtx, typeDef, opts).ToArray();
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
                        ProxyNamespace = proxyNamespace,
                        ProxyMethRef = proxyMethRef,
                    };
                    typeCtx.MethodContexts.Add(methodFullName, methodCtx);
                    #endregion
                }
            }
            if (!asmCtx.TypeContexts.Any())
                return;
        }

        internal void InjectProxyCalls(AssemblyContext asmCtx, InjectedSolution tree)
        {
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                Debug.WriteLine(typeCtx.InjType.FullName);

                //process methods
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    Debug.WriteLine(methodCtx.Method.FullName);

                    var methodDef = methodCtx.Definition;
                    var body = methodDef.Body;
                    //body.SimplifyMacros(); //buggy (Cecil or me?)
                    var instructions = methodCtx.Instructions; //no copy list!

                    #region Jumpers
                    //collect jumpers. Hash table for separate addresses is almost useless,
                    //because they may be recalculated inside the processor during inject...
                    //and ideally, there shouldn't be too many of them 
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var flow = instr.OpCode.FlowControl;
                        if (flow is not (FlowControl.Branch or FlowControl.Cond_Branch))
                            continue;
                        methodCtx.Jumpers.Add(instr);
                        //
                        var anchor = instr.Operand;
                        //need this jump for handle?
                        var curCode = instr.OpCode.Code;
                        //not needed jumps from by Leave from try/catch/finally semantically
                        if (curCode == Code.Leave || curCode == Code.Leave_S)
                            continue;
                        if (instr.Next != anchor && !methodCtx.Anchors.Contains(anchor))
                            methodCtx.Anchors.Add(anchor);
                    }
                    #endregion
                    #region CG method's global call index
                    //these methods are only of the current assembly, but this is enough to work with CG methods
                    //This should be done here, for an already gathered dependency tree
                    var treeMethods = tree.Filter(typeof(InjectedMethod), true)
                        .Cast<InjectedMethod>()
                        .Where(a => a.CalleeIndexes.Count > 0);
                    foreach (var caller in treeMethods)
                    {
                        foreach (var calleName in caller.CalleeIndexes.Keys)
                        {
                            if (asmCtx.InjMethodByFullname.ContainsKey(calleName))
                            {
                                var callee = asmCtx.InjMethodByFullname[calleName];
                                var cgInfo = callee.CGInfo;
                                if (cgInfo == null) //null is normal (business method)
                                    continue;
                                cgInfo.Caller = caller;
                                cgInfo.CallerIndex = caller.CalleeIndexes[calleName];
                            }
                            else { } //hmmm... check, WTF...
                        }
                    }
                    #endregion
                    #region *** Injections ***
                    _strategy.StartMethod(methodCtx); //primary actions
                    for (var i = methodCtx.StartIndex; i < instructions.Count; i++)
                    {
                        #region Checks
                        var instr = instructions[i];
                        if (!methodCtx.BusinessInstructions.Contains(instr))
                            continue;
                        if (methodCtx.AheadProcessed.Contains(instr))
                            continue;
                        #endregion

                        methodCtx.SetPosition(i);
                        i = HandleInstruction(methodCtx); //process and correct current index after potential injection
                    }
                    #endregion
                    #region Correct jumps
                    //EACH short form -> to long form (otherwise, we need to recalculate 
                    //again after each necessary conversion)
                    var jumpers = methodCtx.Jumpers.ToArray();
                    foreach (var jump in jumpers)
                    {
                        var opCode = jump.OpCode;
                        if (jump.Operand is not Instruction)
                            continue;
                        var newOpCode = InstructionHelper.ShortJumpToLong(opCode);
                        if (newOpCode.Code != opCode.Code)
                            jump.OpCode = newOpCode;
                    }
                    #endregion

                    body.Optimize();
                    body.OptimizeMacros();
                }
            }
        }

        internal int HandleInstruction(MethodContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            try
            {
                _strategy.HandleInstruction(ctx);
                return ctx.CurIndex;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Handling instruction: {ctx.ModuleName}; {ctx.Method.FullName}; {nameof(ctx.CurIndex)}: {ctx.CurIndex}");
                throw;
            }
        }

        internal void InjectProxyClass(AssemblyContext asmCtx, string proxyNamespace, InjectorOptions opts)
        {
            //here we generate proxy class which will be calling of real profiler by cached Reflection
            //directory of profiler dependencies - for injected target on it's side
            var module = asmCtx.Module;
            var assembly = asmCtx.Definition;
            var profilerOpts = opts.Profiler;
            var profDir = profilerOpts.Directory;
            var proxyGenerator = new ProfilerProxyGenerator(proxyNamespace, opts.Proxy.Class, opts.Proxy.Method, //proxy to profiler
                                                            profDir, profilerOpts.AssemblyName, //real profiler
                                                            profilerOpts.Namespace, profilerOpts.Class, profilerOpts.Method);
            var isNetFx = asmCtx.Version.Target == AssemblyVersionType.NetFramework;
            proxyGenerator.InjectTo(assembly, isNetFx);

            // ensure we referencing only ref assemblies
            if (isNetFx)
            {
                var systemPrivateCoreLib = module.AssemblyReferences
                    .FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
                //Debug.Assert(systemPrivateCoreLib == null, "systemPrivateCoreLib == null");
                if (systemPrivateCoreLib != null)
                    module.AssemblyReferences.Remove(systemPrivateCoreLib);
            }
        }
    }
}
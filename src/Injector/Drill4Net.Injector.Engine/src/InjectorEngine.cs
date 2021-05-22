using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
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
                ProcessAssembly(runCtx);
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
        
        private void ProcessAssembly(RunContext runCtx)
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

            var sourceDir = asmCtx.SourceDir;
            var destDir = asmCtx.DestinationDir;
            var assembly = asmCtx.Definition;
            var module = asmCtx.Module;
            #endregion
            #region Tree
            var tree = runCtx.Tree;

            //directory
            var treeDir = tree.GetDirectory(sourceDir);
            if (treeDir == null)
            {
                treeDir = new InjectedDirectory(sourceDir, destDir);
                tree.Add(treeDir);
            }

            //assembly (exactly from whole tree, not just current treeDir - for shared dll)
            var treeAsm = tree.GetAssembly(assembly.FullName, true) ??
                          new InjectedAssembly(asmCtx.Version, module.Name, assembly.FullName, filePath);
            treeDir.Add(treeAsm);

            var paths = runCtx.Paths;
            if (paths.ContainsKey(assembly.FullName)) //assembly is shared and already is injected
            {
                var copyFrom = paths[assembly.FullName];
                var copyTo = writer.GetDestFileName(copyFrom, destDir);
                File.Copy(copyFrom, copyTo, true);
                return;
            }
            #endregion
            #region Commands
            // 1. Command ref

            //we will use proxy class (with cached Reflection) leading to real profiler
            //proxy will be inject in each target assembly - let construct the calling of it's method
            var proxyReturnTypeRef = module.TypeSystem.Void;
            var proxyNamespace = $"Injection_{Guid.NewGuid()}".Replace("-", null); //must be unique for each target asm
            var proxyTypeRef = new TypeReference(proxyNamespace, opts.Proxy.Class, module, module);
            var proxyMethRef = new MethodReference(opts.Proxy.Method, proxyReturnTypeRef, proxyTypeRef);
            var strPar = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            proxyMethRef.Parameters.Add(strPar);

            // 2. 'Call' command
            //var call = Instruction.Create(OpCodes.Call, proxyMethRef);
            #endregion
            #region Processing
            var types = TypeHelper.FilterTypes(module.Types, opts.Source.Filter);
            asmCtx.InjAssembly = treeAsm;

            CreateContextData(runCtx, asmCtx, proxyNamespace, proxyMethRef);
            FindMoveNextMethods(asmCtx);
            MapBusinessMethodFirstPass(asmCtx);
            MapBusinessMethodSecondPass(asmCtx);
            CalcBusinessPartCodeSizes(asmCtx);
            Inject(asmCtx, tree);
            CalcCoverageBlocks(asmCtx);
            #endregion
            #region Proxy class
            //here we generate proxy class which will be calling of real profiler by cached Reflection
            //directory of profiler dependencies - for injected target on it's side
            var profilerOpts = opts.Profiler;
            var profDir = profilerOpts.Directory;
            //if (!Directory.EnumerateFiles(profDir).Any() && targetFolder != null)
            //    profDir = Path.Combine(profDir, targetFolder) + "\\";
            var proxyGenerator = new ProfilerProxyGenerator(proxyNamespace, opts.Proxy.Class, opts.Proxy.Method, //proxy to profiler
                                                            profDir, profilerOpts.AssemblyName, //real profiler
                                                            profilerOpts.Namespace, profilerOpts.Class, profilerOpts.Method);
            var isNetFx = asmCtx.Version.Target == AssemblyVersionType.NetFramework;
            proxyGenerator.InjectTo(assembly, isNetFx);
            //
            // ensure we referencing only ref assemblies
            if (isNetFx)
            {
                var systemPrivateCoreLib = module.AssemblyReferences
                    .FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
                //Debug.Assert(systemPrivateCoreLib == null, "systemPrivateCoreLib == null");
                if (systemPrivateCoreLib != null)
                    module.AssemblyReferences.Remove(systemPrivateCoreLib);
            }
            #endregion

            //save modified assembly and symbols to new file    
            var modifiedPath = writer.SaveAssembly(runCtx, asmCtx);
            assembly.Dispose();

            Log.Information($"Modified assembly is created: {modifiedPath}");
        }

        internal void CreateContextData(RunContext runCtx, AssemblyContext asmCtx, string proxyNamespace, MethodReference proxyMethRef)
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

        internal void FindMoveNextMethods(AssemblyContext asmCtx)
        {
            var moveNextMethods = asmCtx.InjAssembly.Filter(typeof(InjectedMethod), true)
               .Cast<InjectedMethod>()
               .Where(x => x.IsCompilerGenerated && x.Name == "MoveNext")
               .ToList();
            for (int i = 0; i < moveNextMethods.Count; i++)
            {
                InjectedMethod meth = moveNextMethods[i];
                // Owner type
                var fullName = meth.FullName;
                var mkey = fullName.Split(' ')[1].Split(':')[0];
                if (asmCtx.InjClasses.ContainsKey(mkey))
                {
                    var treeType = asmCtx.InjClasses[mkey];
                    treeType.Add(meth);
                }
                // Business method
                var extRealMethodName = MethodHelper.TryGetBusinessMethod(meth.FullName, meth.Name, true, true);
                mkey = MethodHelper.GetMethodKey(meth.TypeName, extRealMethodName);
                if (!asmCtx.InjMethodByKeys.ContainsKey(mkey))
                    continue;
                var treeFunc = asmCtx.InjMethodByKeys[mkey];
                if (meth.CGInfo != null)
                    meth.CGInfo.FromMethod = treeFunc.FullName;
            }
        }

        internal void MapBusinessMethodFirstPass(AssemblyContext asmCtx)
        {
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    #region Init
                    var startInd = methodCtx.StartIndex;
                    var instructions = methodCtx.Instructions;
                    var injMethod = methodCtx.Method;

                    var cgInfo = injMethod.CGInfo;
                    if (cgInfo != null)
                        cgInfo.FirstIndex = startInd == 0 ? 0 : startInd - 1; //correcting to real start
                    #endregion

                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        methodCtx.SetPosition(i);
                        MethodHelper.MapBusinessFunction(methodCtx); //in any case check each instruction for mapping

                        #region Check
                        var checkRes = CheckInstruction(methodCtx);
                        if (checkRes == FlowType.NextCycle)
                            continue;
                        if (checkRes == FlowType.BreakCycle)
                            break;
                        if (checkRes == FlowType.Return)
                            return;
                        #endregion

                        i = methodCtx.CurIndex; //because it can change
                        methodCtx.BusinessInstructions.Add(instructions[i]);
                    }
                    //
                    var cnt = methodCtx.BusinessInstructions.Count;
                    injMethod.BusinessSize = cnt;
                    injMethod.OwnBusinessSize = cnt;
                }
            }
        }

        internal void MapBusinessMethodSecondPass(AssemblyContext asmCtx)
        {
            var badCtxs = new List<MethodContext>();
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    var meth = methodCtx.Method;
                    if (meth.IsCompilerGenerated)
                    {
                        var methFullName = meth.FullName;
                        if (meth.BusinessMethod == methFullName)
                        {
                            var bizFunc = typeCtx.MethodContexts.Values.Select(a => a.Method)
                                .FirstOrDefault(a => a.CalleeIndexes.ContainsKey(methFullName));
                            if (bizFunc != null)
                                meth.CGInfo.Caller = bizFunc;
                            else
                                badCtxs.Add(methodCtx);
                        }
                    }
                }
            }

            // the removing methods for which business method is not defined 
            foreach (var ctx in badCtxs)
                ctx.TypeCtx.MethodContexts.Remove(ctx.Method.FullName);
        }

        internal void CalcBusinessPartCodeSizes(AssemblyContext asmCtx)
        {
            var bizMethods = asmCtx.InjMethodByFullname.Values
                .Where(a => !a.IsCompilerGenerated).ToArray();
            if (!bizMethods.Any())
                return;
            foreach (var caller in bizMethods.Where(a => a.CalleeIndexes.Count > 0))
            {
                foreach (var calleeName in caller.CalleeIndexes.Keys)
                    MethodHelper.CorrectMethodBusinessSize(asmCtx.InjMethodByFullname, caller, calleeName);
            }
        }

        internal void Inject(AssemblyContext asmCtx, InjectedSolution tree)
        {
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                Debug.WriteLine(typeCtx.InjType.FullName);

                //process methods
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    Debug.WriteLine(methodCtx.Method.FullName);

                    #region Init
                    var methodDef = methodCtx.Definition;

                    //instructions
                    var body = methodDef.Body;
                    //body.SimplifyMacros(); //buggy (Cecil or me?)
                    var instructions = methodCtx.Instructions; //no copy list!
                    #endregion
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

        internal void CalcCoverageBlocks(AssemblyContext asmCtx)
        {
            var allMethods = asmCtx.InjMethodByFullname.Values;
            foreach (var method in allMethods)
            {
                var points = method.Points;
                var ranges = points
                    .Select(a => a.BusinessIndex)
                    .Where(c => c != 0) //Enter not needed in any case (for the block type of coverage)
                    .OrderBy(b => b)
                    .Distinct() //need for exclude in fact some fictive (for coverage) injections: CycleEnd, etc
                    .ToList();
                if (!ranges.Any())
                    continue;
                //
                var coverage = method.Coverage;
                foreach (var ind in ranges)
                {
                    var points2 = points.Where(a => a.BusinessIndex == ind).ToList();
                    if (points2.Count() > 1)
                        points2 = points2.Where(a => a.PointType != CrossPointType.CycleEnd).ToList(); //Guanito...
                    coverage.PointToBlockEnds.Add(points2[0].PointUid, ind);
                }

                //by parts
                float origSize = ranges.Last() + 1;
                var prev = -1;
                foreach (var range in ranges)
                {
                    coverage.BlockByPart.Add(range, (range - prev) / origSize);
                    prev = range;
                }
                //
                var sum = coverage.BlockByPart.Values.Sum(); //must be 1.0
                if (Math.Abs(sum - 1) > 0.0001)
                {
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

        internal FlowType CheckInstruction(MethodContext ctx)
        {
            var instr = ctx.CurInstruction;
            var code = instr.OpCode.Code;

            // for injecting cases
            if (code == Code.Nop || ctx.AheadProcessed.Contains(instr))
                return FlowType.NextCycle;

            var method = ctx.Method;
            
            #region Awaiters in MoveNext as a boundary
            //find the boundary between the business code and the compiler-generated one
            if (method.Source.IsMoveNext && method.Source.MethodType == MethodType.CompilerGenerated)
            {
                if (IsAsyncCompletedOperand(instr))
                {
                    //leave the current first try/catch
                    var res = LeaveTryCatch(ctx);

                    //is second 'get_IsCompleted' exists(for example, for 'async stream' method)?
                    if (res)
                    {
                        var instructions = ctx.Instructions;
                        for (var i = ctx.CurIndex + 1; i < instructions.Count; i++)
                        {
                            if (!IsAsyncCompletedOperand(instructions[i]))
                                continue;
                            ctx.SetPosition(i);
                            LeaveTryCatch(ctx);
                        }
                    }
                }
                else
                {
                    //+margin if instruction starts the try/catch
                    var delta = ctx.ExceptionHandlers.Any(a => a.TryStart == instr) ? 6 : 0;
                    ctx.CorrectIndex(delta);
                }
            }
            #endregion

            return FlowType.NextOperand;

            //local funcs
            static bool LeaveTryCatch(MethodContext ctx)
            {
                var instr = ctx.CurInstruction;
                var curTryCactches = ctx.ExceptionHandlers.Where(a => a.TryStart.Offset < instr.Offset && instr.Offset < a.HandlerEnd.Offset);
                if (curTryCactches.Any())
                {
                    var maxStart = curTryCactches.Max(a => a.TryStart.Offset);
                    var curTryCactch = curTryCactches.First(b => b.TryStart.Offset == maxStart);
                    var endInd = ctx.Instructions.IndexOf(curTryCactch.HandlerEnd) + 1;
                    ctx.SetPosition(endInd);
                    return true;
                }
                return false;
            }

            static bool IsAsyncCompletedOperand(Instruction instr)
            {
                return instr.Operand?.ToString().Contains("::get_IsCompleted()") == true;
            }
        }
    }
}
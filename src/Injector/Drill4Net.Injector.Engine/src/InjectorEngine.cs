using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Globalization;
using System.Runtime.Versioning;
using Serilog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Injection;
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
        private readonly ThreadLocal<bool?> _isNetCore;
        private readonly ThreadLocal<AssemblyVersioning> _mainVersion;
        private readonly InstructionHandlerStrategy _strategy;
        private readonly TypeChecker _typeChecker;

        /***************************************************************************************/

        public InjectorEngine(IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _isNetCore = new ThreadLocal<bool?>();
            _mainVersion = new ThreadLocal<AssemblyVersioning>();
            _typeChecker = new TypeChecker();

            FlowStrategy flowStrategy = new();
            _strategy = flowStrategy;
        }

        /***************************************************************************************/

        public InjectedSolution Process()
        {
            return Process(_rep.Options);
        }

        public InjectedSolution Process(MainOptions opts)
        {
            Log.Information("Process starting...");
            OptionHelper.ValidateOptions(opts);

            var sourceDir = opts.Source.Directory;
            var destDir = opts.Destination.Directory;

            //copying of all needed data in needed targets
            var monikers = opts.Tests?.Targets;
            _rep.CopySource(sourceDir, destDir, monikers);

            var versions = DefineTargetVersions(sourceDir);
            
            //tree
            var tree = new InjectedSolution(opts.Target?.Name, sourceDir)
            {
                StartTime = DateTime.Now,
                DestinationPath = destDir,
            };

            //targets from in cfg
            var dirs = Directory.GetDirectories(sourceDir, "*");
            foreach (var dir in dirs)
            {
                var yes = monikers == null || monikers.Count == 0 || monikers.Any(a =>
                {
                    var x = Path.Combine(sourceDir, a.Value.BaseFolder);
                    if (x.EndsWith("\\"))
                        x = x.Substring(0, x.Length - 1);
                    var z = Path.Combine(dir, a.Key);
                    return x == z;
                });

                if(yes) 
                    ProcessDirectory(dir, versions, opts, tree);
            }

            //copying tree data to target root directories
            InjectTree(tree);
            tree.FinishTime = DateTime.Now;
            
            // debug
            //var methods = tree.GetAllMethods().ToList();
            //var cgMeths = methods.Where(a => a.IsCompilerGenerated).ToList();
            //var emtyCGInfoMeths = cgMeths
            //    .Where(a => a.CGInfo == null)
            //    .ToList();
            //var emptyBusinessMeths = cgMeths
            //    .Where(a => a.CGInfo!= null && a.CGInfo.Caller != null && (a.BusinessMethod == null || a.BusinessMethod == a.FullName))
            //    .ToList();
            //var nonBlokings = cgMeths.FirstOrDefault(a => a.FullName == "System.String Drill4Net.Target.Common.InjectTarget/<>c::<Async_Linq_NonBlocking>b__54_0(Drill4Net.Target.Common.GenStr)");
            //
            return tree;
        }

        internal void ProcessDirectory(string directory, Dictionary<string, AssemblyVersioning> versions, 
             MainOptions opts, InjectedSolution tree)
        {
            //files
            var files = _rep.GetAssemblies(directory);
            foreach (var file in files)
            {
                ProcessAssembly(file, versions, opts, tree);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*");
            foreach (var dir in dirs)
            {
                ProcessDirectory(dir, versions, opts, tree);
            }
        }

        internal Dictionary<string, AssemblyVersioning> DefineTargetVersions(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Source directory not exists: [{directory}]");
            var files = _rep.GetAssemblies(directory);
            var versions = new Dictionary<string, AssemblyVersioning>();
            //'exe' must be after 'dll'
            foreach (var file in files.OrderBy(a => a))
            {
                AssemblyVersioning version;
                if (_isNetCore.Value == true && Path.GetExtension(file) == ".exe")
                {
                    var dll = Path.Combine(Path.ChangeExtension(file, ".dll"));
                    var dllVer = versions.FirstOrDefault(a => a.Key == dll).Value;
                    version = dllVer ?? new AssemblyVersioning() { Target = AssemblyVersionType.NetCore };
                    _mainVersion.Value = version;
                    versions.Add(file, version);
                    continue;
                }
                version = _rep.GetAssemblyVersion(file);
                versions.Add(file, version);
                //
                if (_isNetCore.Value == null)
                {
                    switch (version.Target)
                    {
                        case AssemblyVersionType.NetCore:
                            _mainVersion.Value = version;
                            _isNetCore.Value = true;
                            break;
                        case AssemblyVersionType.NetFramework:
                            _mainVersion.Value = version;
                            _isNetCore.Value = false;
                            break;
                    }
                }
            }
            return versions;
        }
        
        private void ProcessAssembly(string filePath, Dictionary<string, AssemblyVersioning> versions,  
            MainOptions opts, InjectedSolution tree)
        {
            #region Reading
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");

            //filter
            if (!_typeChecker.CheckByAssemblyPath(filePath))
                return;

            //source
            var sourceDir = $"{Path.GetFullPath(Path.GetDirectoryName(filePath) ?? string.Empty)}\\";
            Environment.CurrentDirectory = sourceDir;
            var subjectName = Path.GetFileNameWithoutExtension(filePath);

            //destination
            var destDir = FileUtils.GetDestinationDirectory(opts, sourceDir);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            //must process? need to know the version of the assembly before reading it via cecil
            #region Version
            var ext = Path.GetExtension(filePath);
            AssemblyVersioning version;
            if (versions.ContainsKey(filePath))
            {
                version = versions[filePath];
            }
            else
            {
                version = _rep.GetAssemblyVersion(filePath);
                if (version != null)
                    versions.Add(filePath, version);
            }
            if (version == null || version.Target == AssemblyVersionType.NotIL || 
                (ext == ".exe" && version.Target == AssemblyVersionType.NetCore))
                return;
            Console.WriteLine($"Version = {version}");
            #endregion
            #region Read params
            var readerParams = new ReaderParameters
            {
                // we will write to another file, so we don't need this
                ReadWrite = false,
                // read everything at once
                ReadingMode = ReadingMode.Immediate,
            };

            #region PDB
            var pdb = $"{subjectName}.pdb";
            var isPdbExists = File.Exists(pdb);
            var needPdb = isPdbExists && (version.Target is AssemblyVersionType.NetCore or AssemblyVersionType.NetStandard);
            if (needPdb)
            {
                // netcore uses portable pdb, so we provide appropriate reader
                readerParams.SymbolReaderProvider = new PortablePdbReaderProvider();
                readerParams.ReadSymbols = true;
                try
                {
                    readerParams.SymbolStream = File.Open(pdb, FileMode.Open);
                }
                catch (IOException ex) //may be in VS for NET Core .exe
                {
                    if(!Debugger.IsAttached)
                       // Log.Warning(ex, $"Reading PDB (from IDE): {nameof(ProcessAssembly)}");
                    //else
                        Log.Error(ex, $"Reading PDB: {nameof(ProcessAssembly)}");
                }
            }
            #endregion
            #endregion

            // read subject assembly with symbols
            using var assembly = AssemblyDefinition.ReadAssembly(filePath, readerParams);
            var module = assembly.MainModule;
            var moduleName = module.Name;
            #endregion
            #region Tree
            //directory
            var treeDir = tree.GetDirectory(sourceDir);
            if (treeDir == null)
            {
                treeDir = new InjectedDirectory(sourceDir, destDir);
                tree.AddChild(treeDir);
            }

            //assembly
            var treeAsm = treeDir.GetAssembly(assembly.FullName);
            if (treeAsm == null)
            {
                treeAsm = new InjectedAssembly(version, module.Name, assembly.FullName, filePath);
                treeDir.AddChild(treeAsm);
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
            var types = FilterTypes(module.Types);
            var asmCtx = new AssemblyContext(filePath, version, assembly, treeAsm);

            #region 1. Tree's entities & Contexts
            #region Creating contexts
            //by type
            foreach (var typeDef in types)
            {
                var typeFullName = typeDef.FullName;
                
                #region Tree
                var realTypeName = TryGetRealTypeName(typeDef);
                var treeMethodType = new InjectedType(treeAsm.Name, typeFullName, realTypeName)
                {
                    Source = CreateTypeSource(typeDef),
                    Path = treeAsm.Path,
                };
                asmCtx.InjClasses.Add(treeMethodType.FullName, treeMethodType);
                treeAsm.AddChild(treeMethodType);
                #endregion

                var typeCtx = new TypeContext(asmCtx, typeDef, treeMethodType);
                asmCtx.TypeContexts.Add(typeFullName, typeCtx);
                
                //collect methods including business & compiler's nested classes
                //together (for async, delegates, anonymous types...)
                var methods = GetMethods(typeCtx, typeDef, opts);

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
                    var isSpecFunc = IsSpecialGeneratedMethod(methodType);
                    var strictEnterReturn = //what is principally forbidden
                        !isSpecFunc &&
                        (                           
                            methodName.Contains("|") || //local func                                                        
                            isAsyncStateMachine || //async/await
                            isCompilerGenerated ||
                            //Finalize() -> strange, but for Core 'Enter' & 'Return' lead to a crash                   
                            (_isNetCore.Value == true && methodSource.IsFinalizer)
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
                                if (curAsyncCode is Code.Nop or Code.Stfld || curAsyncCode.ToString().StartsWith("Ldarg"))
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
            #endregion
            #region MoveNext methods
            var moveNextMethods = treeAsm.Filter(typeof(InjectedMethod), true)
                .Cast<InjectedMethod>()
                .Where(x => x.IsCompilerGenerated && x.Name == "MoveNext");
            foreach (var meth in moveNextMethods)
            {
                // Owner type
                var fullName = meth.FullName;
                var mkey = fullName.Split(' ')[1].Split(':')[0];
                if (asmCtx.InjClasses.ContainsKey(mkey))
                {
                    var treeType = asmCtx.InjClasses[mkey];
                    treeType.AddChild(meth);
                }
                // Business method
                var extRealMethodName = TryGetBusinessMethod(meth.FullName, meth.Name, true, true);
                mkey = GetMethodKey(meth.TypeName, extRealMethodName);
                if (!asmCtx.InjMethodByKeys.ContainsKey(mkey)) 
                    continue;
                var treeFunc = asmCtx.InjMethodByKeys[mkey];
                if (meth.CGInfo != null)
                   meth.CGInfo.FromMethod = treeFunc.FullName;
            }
            #endregion
            #region Business function mapping (first pass)
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
                        MapBusinessFunction(methodCtx); //in any case check each instruction for mapping

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
            #endregion
            #region Business function mapping (second pass)
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
            #endregion
            #region Size of the business code parts
            var bizMethods = asmCtx.InjMethodByFullname.Values
                .Where(a => !a.IsCompilerGenerated).ToArray();
            foreach (var caller in bizMethods.Where(a => a.CalleeIndexes.Count > 0))
            {
                foreach (var calleeName in caller.CalleeIndexes.Keys)
                    CorrectMethodBusinessSize(asmCtx.InjMethodByFullname, caller, calleeName);
            }
            #endregion
            #endregion
            #region 2. Injection
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                //process methods
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
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
                    #region Injections
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
                        var newOpCode = ConvertShortJumpToLong(opCode);
                        if (newOpCode.Code != opCode.Code)
                            jump.OpCode = newOpCode;
                    }
                    #endregion

                    body.Optimize();
                    body.OptimizeMacros();
                }
            }
            #endregion
            #region 3. Coverage blocks for the methods
            foreach (var bizMethod in bizMethods)
            {
                var points = bizMethod.Points;
                var ranges = points
                    .Select(a => a.BusinessIndex)
                    .Where(c => c != 0) //Enter not needed in any case
                    .OrderBy(b => b)
                    .Distinct() //need for exclude in fact some fictive (for coverage) injections: CycleEnd, etc
                    .ToList();
                if (!ranges.Any())
                    continue;
                //
                var coverage = bizMethod.Coverage;
                foreach (var ind in ranges)
                {
                    var points2 = points.Where(a => a.BusinessIndex == ind).ToList();
                    if (points2.Count() > 1)
                        points2 = points2.Where(a => a.PointType != CrossPointType.CycleEnd).ToList(); //Guanito...
                    coverage.PointUidToEndIndex.Add(points2[0].PointUid, ind);
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
            #endregion
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
            var isNetFx = version.Target == AssemblyVersionType.NetFramework;
            proxyGenerator.InjectTo(assembly, isNetFx);
            //
            // ensure we referencing only ref assemblies
            if (isNetFx)
            {
                var systemPrivateCoreLib = module.AssemblyReferences.FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
                //Debug.Assert(systemPrivateCoreLib == null, "systemPrivateCoreLib == null");
                if (systemPrivateCoreLib != null)
                    module.AssemblyReferences.Remove(systemPrivateCoreLib);
            }
            #endregion

            //save modified assembly and symbols to new file    
            var modifiedPath = SaveAssembly(assembly, filePath, destDir, needPdb);
            assembly.Dispose();

            Log.Information($"Modified assembly is created: {modifiedPath}");
        }

        internal void CorrectMethodBusinessSize(Dictionary<string, InjectedMethod> methods, InjectedMethod caller, 
            string calleeName)
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
            foreach(var subCalleeName in callee.CalleeIndexes.Keys)
            {
                CorrectMethodBusinessSize(methods, callee, subCalleeName);
            }
            
            //the size of caller consists of own size + all sizes of it's CG callees
            //(already included in them)
            caller.BusinessSize += callee.BusinessSize;
        }

        /// <summary>
        /// Get all types of assembly for filtering
        /// </summary>
        /// <param name="allTypes"></param>
        /// <returns></returns>
        internal IEnumerable<TypeDefinition> FilterTypes(IEnumerable<TypeDefinition> allTypes)
        {
            var res = new List<TypeDefinition>();
            foreach (var typeDef in allTypes)
            {
                var typeName = typeDef.Name;
                if (typeName == "<Module>")
                    continue;

                //TODO: normal defining of business types (by cfg?)
                //var nameSpace = typeDef.Namespace;
                var typeFullName = typeDef.FullName;
                if (!_typeChecker.CheckByNamespace(typeFullName))
                    continue;
                res.Add(typeDef);
            }
            return res;
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
                if (IsFoundIsCompleted(instr))
                {
                    //leave the current first try/catch
                    var res = LeaveTryCatch(ctx);

                    //is second 'get_IsCompleted' exists(for example, for 'async stream' method)?
                    if (res)
                    {
                        var instructions = ctx.Instructions;
                        for (var i = ctx.CurIndex + 1; i < instructions.Count; i++)
                        {
                            if (!IsFoundIsCompleted(instructions[i]))
                                continue;
                            ctx.SetPosition(i);
                            LeaveTryCatch(ctx);
                        }
                    }
                }
                else
                {
                    //+margin if instruction starts the try/catch
                    var delta = ctx.ExceptionHandlers.Any(a => a.TryStart == instr) ? 8 : 0;
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

            static bool IsFoundIsCompleted(Instruction instr)
            {
                return instr.Operand?.ToString().Contains("::get_IsCompleted()") == true;
            }
        }

        internal OpCode ConvertShortJumpToLong(OpCode opCode)
        {
            //TODO: to a dictionary
            return opCode.Code switch
            {
                Code.Br_S => OpCodes.Br,
                Code.Brfalse_S => OpCodes.Brfalse,
                Code.Brtrue_S => OpCodes.Brtrue,
                Code.Beq_S => OpCodes.Beq,
                Code.Bge_S => OpCodes.Bge,
                Code.Bge_Un_S => OpCodes.Bge_Un,
                Code.Bgt_S => OpCodes.Bgt,
                Code.Bgt_Un_S => OpCodes.Bgt_Un,
                Code.Ble_S => OpCodes.Ble,
                Code.Ble_Un_S => OpCodes.Ble_Un,
                Code.Blt_S => OpCodes.Blt,
                Code.Blt_Un_S => OpCodes.Blt_Un,
                Code.Bne_Un_S => OpCodes.Bne_Un,
                Code.Leave_S => OpCodes.Leave,
                _ => opCode,
            };
        }

        internal void MapBusinessFunction(MethodContext ctx)
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
                            var nestType = (InjectedType) injectedSimpleEntity;
                            if (nestType.IsCompilerGenerated)
                                nestType.FromMethod = treeFunc.BusinessMethod;
                        }
                    }

                    extType.FromMethod = treeFunc.FullName;
                    //better process all methods, not filter only extName... may be... check it!
                    var extMethods = extType.Filter(typeof(InjectedMethod), true);
                    foreach (var injectedSimpleEntity in extMethods)
                    {
                        var meth = (InjectedMethod) injectedSimpleEntity;
                        if (meth.CGInfo!=null)
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

        internal string SaveAssembly(AssemblyDefinition assembly, string origFilePath, string destDir, bool needPdb)
        {
            var ext = Path.GetExtension(origFilePath);
            var subjectName = Path.GetFileNameWithoutExtension(origFilePath);
            var modifiedPath = $"{Path.Combine(destDir, subjectName)}{ext}";

            var writeParams = new WriterParameters();
            if (needPdb)
            {
                var pdbPath = Path.Combine(destDir, subjectName + ".pdb");
                writeParams.SymbolStream = File.Create(pdbPath);
                writeParams.WriteSymbols = true;
                // net core uses portable pdb
                writeParams.SymbolWriterProvider = new PortablePdbWriterProvider();
            }
            assembly.Write(modifiedPath, writeParams);
            return modifiedPath;
        }

        internal string ConvertTargetTypeToFolder(string fullType)
        {
            if (fullType.StartsWith(".NETFramework"))
                return null;
            var ar = fullType.Split('=');
            var version = ar[1].Replace("v", null);
            var digit = float.Parse(version, CultureInfo.InvariantCulture);
            if (fullType.StartsWith(".NETStandard"))
                return $"netstandard{version}";
            if (fullType.StartsWith(".NETCoreApp"))
                return digit < 5 ? $"netcoreapp{version}" : $"net{version}";
            return null;
        }

        internal string TryGetBusinessMethod(string typeName, string methodName, bool isCompilerGenerated, 
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
            catch(Exception ex)
            {
                Log.Error(ex,nameof(TryGetBusinessMethod));
            }
            return realMethodName;
        }

        internal string TryGetRealTypeName(TypeDefinition type)
        {
            if (type?.DeclaringType == null) 
                return type?.FullName;
            do type = type.DeclaringType;
                while (type.DeclaringType != null && (type.Name.Contains("c__DisplayClass") || type.Name.Contains("<>")));
            return type?.FullName;
        }

        internal MethodType GetMethodType(MethodDefinition def)
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

        internal bool IsSpecialGeneratedMethod(MethodType type)
        {
            return type is MethodType.EventAdd or MethodType.EventRemove;
        }

        internal IEnumerable<MethodDefinition> GetMethods(TypeContext typeCtx, TypeDefinition type, MainOptions opts)
        {
            #region Own methods
            #region Filter methods
            var probOpts = opts.Probes;
            var isAngleBracket = type.Name.StartsWith("<");
            var ownMethods = type.Methods
                    .Where(a => a.HasBody)
                    .Where(a => !(isAngleBracket && a.IsConstructor)) //internal compiler's ctor is not needed in any cases
                    .Where(a => probOpts.Ctor || (!probOpts.Ctor && !a.IsConstructor)) //may be we skips own ctors
                    .Where(a => probOpts.Setter || (!probOpts.Setter && a.Name != "set_Prop")) //do we need property setters?
                    .Where(a => probOpts.Getter || (!probOpts.Getter && a.Name != "get_Prop")) //do we need property getters?
                    .Where(a => probOpts.EventAdd || !(a.FullName.Contains("::add_") && !probOpts.EventAdd)) //do we need 'event add'?
                    .Where(a => probOpts.EventRemove || !(a.FullName.Contains("::remove_") && !probOpts.EventRemove)) //do we need 'event remove'?
                    .Where(a => isAngleBracket || !a.IsPrivate || !(a.IsPrivate && !probOpts.Private)) //do we need business privates?
                ;
            #endregion
            
            var treeParentClass = typeCtx.InjType;
            var asmCtx = typeCtx.AssemblyCtx;
            
            //check for type's characteristics
            var interfaces = type.Interfaces;
            var isAsyncStateMachine =
                type.Methods.FirstOrDefault(a => a.Name == "SetStateMachine") != null ||
                interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
            var isEnumerable = interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;
            var typeFullname = treeParentClass.FullName;
            
            var methods = new List<MethodDefinition>();
            foreach (var ownMethod in ownMethods)
            {
                #region Check
                //check for setter & getter of properties for anonymous types
                //is it useless? But for custom weaving it's very interesting idea...
                if (treeParentClass.IsCompilerGenerated)
                {
                    var name = ownMethod.Name;
                    if (ownMethod.IsSetter && !name.StartsWith("set_Prop"))
                        continue;
                    if (ownMethod.IsGetter && !name.StartsWith("get_Prop"))
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

                // //debug
                // var funcs = treeParentClass.Filter(typeof(InjectedMethod), true)
                //     .Cast<InjectedMethod>()
                //     .Select(a => a.FullName).ToList();
                // var func = funcs
                //     .FirstOrDefault(a => a == treeFunc.FullName);
                
                treeParentClass.AddChild(treeFunc);
            }
            #endregion
            #region Nested classes
            foreach (var nestedType in type.NestedTypes)
            {
                var realTypeName = TryGetRealTypeName(nestedType);
                var treeType = new InjectedType(nestedType.Module.Name, nestedType.FullName, realTypeName)
                {
                    Source = CreateTypeSource(nestedType),
                    Path = typeCtx.AssemblyCtx.InjAssembly.Path,
                };
                asmCtx.InjClasses.Add(treeType.FullName, treeType);
                treeParentClass.AddChild(treeType);
                //
                var innerMethods = GetMethods(typeCtx, nestedType, opts);
                methods.AddRange(innerMethods);
            }
            #endregion

            methods = methods
                .Where(m => m.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType.Name == nameof(DebuggerHiddenAttribute)) == null)
                .ToList();
            return methods;
        }

        private string GetMethodKey(string typeFullname, string methodShortName)
        {
            return $"{typeFullname}::{methodShortName}";
        }

        private TypeSource CreateTypeSource(TypeDefinition def)
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

        private MethodSource CreateMethodSource(MethodDefinition def)
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

        private AccessType GetAccessType(MethodDefinition def)
        {
            if (def.IsPrivate)
                return AccessType.Private;
            if (def.IsPublic)
                return AccessType.Public;
            return AccessType.Internal;
        }

        private AccessType GetAccessType(TypeDefinition def)
        {
            if (def.IsNestedPrivate)
                return AccessType.Private;
            if (def.IsPublic)
                return AccessType.Public;
            return AccessType.Internal;
        }

        internal string GetMethodHashCode(Mono.Collections.Generic.Collection<Instruction> instructions)
        {
            var s = "";
            foreach (var p in instructions)
                s += p.ToString();
            return s.GetHashCode().ToString();
        }

        #region Tree
        internal void InjectTree(InjectedSolution tree)
        {
            SaveTree(tree);
            NotifyAboutTree(tree);
        }

        internal void SaveTree(InjectedSolution tree)
        {
            var path = _rep.GetTreeFilePath(tree);
            _rep.WriteInjectedTree(path, tree);
        }

        internal void NotifyAboutTree(InjectedSolution tree)
        {
            //in each folder create file with path to tree data
            var dirs = tree.GetAllDirectories().ToList();
            if (!dirs.Any()) 
                return;
            var pathInText = _rep.GetTreeFilePath(tree);
            Log.Debug($"Tree saved to: [{pathInText}]");
            foreach (var dir in dirs)
            {
                var hintPath = _rep.GetTreeFileHintPath(dir.DestinationPath);
                File.WriteAllText(hintPath, pathInText);
                Log.Debug($"Hint placed to: [{hintPath}]");
            }
        }
        #endregion
    }
}
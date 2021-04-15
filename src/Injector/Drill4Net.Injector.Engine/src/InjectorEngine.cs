using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Xml.Serialization;
using Serilog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Injection;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Strategies.Flow;
using Drill4Net.Injector.Strategies.Block;
using Mono.Collections.Generic;

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
        private readonly ThreadLocal<AssemblyVersion> _mainVersion;
        private readonly InstructionHandlerStrategy _strategy;
        private readonly TypeChecker _typeChecker;

        /***************************************************************************************/

        public InjectorEngine(IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _isNetCore = new ThreadLocal<bool?>();
            _mainVersion = new ThreadLocal<AssemblyVersion>();
            _typeChecker = new TypeChecker();

            FlowStrategy flowStrategy = new();
            BlockStrategy blockStrategy = new();
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
            //
            return tree;
        }

        internal void ProcessDirectory(string directory, Dictionary<string, AssemblyVersion> versions, 
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

        internal Dictionary<string, AssemblyVersion> DefineTargetVersions(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Source directory not exists: [{directory}]");
            var files = _rep.GetAssemblies(directory);
            var versions = new Dictionary<string, AssemblyVersion>();
            //'exe' must be after 'dll'
            foreach (var file in files.OrderBy(a => a))
            {
                AssemblyVersion version;
                if (_isNetCore.Value == true && Path.GetExtension(file) == ".exe")
                {
                    var dll = Path.Combine(Path.ChangeExtension(file, ".dll"));
                    var dllVer = versions.FirstOrDefault(a => a.Key == dll).Value;
                    version = dllVer ?? new AssemblyVersion() { Target = AssemblyVersionType.NetCore };
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

        private void ProcessAssembly(string filePath, Dictionary<string, AssemblyVersion> versions,  
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
            var ext = Path.GetExtension(filePath);
            var version = versions.ContainsKey(filePath) ? versions[filePath] : _rep.GetAssemblyVersion(filePath);
            if (version == null || (ext == ".exe" && version.Target == AssemblyVersionType.NetCore))
                return;

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
            #region Target version
            var targetVersionAtr = assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == nameof(TargetFrameworkAttribute));
            string targetVersion;
            if (targetVersionAtr != null)
            {
                targetVersion = targetVersionAtr.ConstructorArguments[0].Value?.ToString();
                Console.WriteLine($"Version = {targetVersion}");
            }
            //var targetFolder = ConvertTargetTypeToFolder(targetVersion);
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
                treeAsm = new InjectedAssembly(module.Name, assembly.FullName, filePath);
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
                    SourceType = CreateTypeSource(typeDef)
                };
                asmCtx.InjClasses.Add(treeMethodType.Fullname, treeMethodType);
                treeAsm.AddChild(treeMethodType);
                #endregion

                var typeCtx = new TypeContext(asmCtx, typeDef, treeMethodType);
                asmCtx.TypeContexts.Add(typeCtx);
                
                //collect methods including business & compiler's nested classes
                //together (for async, delegates, anonymous types...)
                var methods = GetMethods(typeCtx, typeDef, opts);

                //by methods
                foreach (var methodDef in methods)
                {
                    #region Init
                    var curType = methodDef.DeclaringType;
                    typeFullName = curType.FullName;

                    var methodName = methodDef.Name;
                    var methodFullName = methodDef.FullName;

                    //Tree
                    treeMethodType = asmCtx.InjClasses[typeFullName];
                    var typeSource = treeMethodType.SourceType;
                    var treeFunc = asmCtx.InjMethodByFullname[methodFullName];
                    var methodSource = treeFunc.SourceType;
                    var methodType = methodSource.MethodType;

                    //the CompilerGeneratedAttribute itself is not enough!
                    //not use isMoveNext - this class may be own iterator, not compiler's one
                    var isCompilerGenerated = methodType == MethodType.CompilerGeneratedPart;

                    var isAsyncStateMachine = typeSource.IsAsyncStateMachine;
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
                            for (var i = 0; i < 3 && asyncInstr.Next != null; i++)
                                asyncInstr = asyncInstr.Next;
                            while (true)
                            {
                                if (asyncInstr.OpCode.FlowControl == FlowControl.Next || asyncInstr.Next == null)
                                    break;
                                asyncInstr = asyncInstr.Next;
                            }
                            startInd = instructions.IndexOf(asyncInstr) + 1;
                        }
                        else
                        {
                            startInd = 12;
                        }
                    }
                    #endregion
                    #region Method context
                    var methodCtx = new MethodContext(typeCtx, methodDef, instructions, processor)
                    {
                        Type = treeMethodType,
                        Method = treeFunc,
                        StartIndex = startInd,
                        ExceptionHandlers = body.ExceptionHandlers,
                        IsStrictEnterReturn = strictEnterReturn,
                        ProxyNamespace = proxyNamespace,
                        ProxyMethRef = proxyMethRef,
                    };
                    typeCtx.MethodContexts.Add(methodCtx);
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
                var fullName = meth.Fullname;
                var mkey = fullName.Split(' ')[1].Split(':')[0];
                if (asmCtx.InjClasses.ContainsKey(mkey))
                {
                    var treeType = asmCtx.InjClasses[mkey];
                    treeType.AddChild(meth);
                }
                // Business method
                 var extRealMethodName = TryGetBusinessMethod(meth.Fullname, meth.Name, true, true);
                 mkey = GetMethodKey(meth.TypeName, extRealMethodName);
                 if (!asmCtx.InjMethodByKeys.ContainsKey(mkey)) 
                     continue;
                 var treeFunc = asmCtx.InjMethodByKeys[mkey];
                 meth.FromMethod = treeFunc.Fullname;
            }
            #endregion
            #region Business function mapping
            foreach (var typeCtx in asmCtx.TypeContexts)
            {
                foreach (var methodCtx in typeCtx.MethodContexts)
                {
                    #region Init
                    var startInd = methodCtx.StartIndex;
                    var instructions = methodCtx.Instructions;

                    var cgInfo = methodCtx.Method.CompilerGeneratedInfo;
                    if (cgInfo != null)
                        cgInfo.FirstIndex = startInd == 0 ? 0 : startInd - 1; //correcting to real start
                    #endregion

                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        methodCtx.SetIndex(i);
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
                    if (cgInfo != null)
                        cgInfo.LastIndex = methodCtx.SourceIndex;
                }
            }
            #endregion
            #endregion
            var a = 1;
            #region 2. Injection
            foreach (var typeCtx in asmCtx.TypeContexts)
            {
                //process methods
                foreach (var methodCtx in typeCtx.MethodContexts)
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
                        //pseudo-jump?
                        if(instr.Next != anchor && !methodCtx.Anchors.Contains(anchor)) 
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
                                var cgInfo = callee.CompilerGeneratedInfo;
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
                        if (methodCtx.Processed.Contains(instr))
                            continue;
                        #endregion
                        
                        methodCtx.SetIndex(i);
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

        /// <summary>
        /// Get all types of assembly for filtering
        /// </summary>
        /// <param name="allTypes"></param>
        /// <returns></returns>
        internal IEnumerable<TypeDefinition> FilterTypes(Collection<TypeDefinition> allTypes)
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
                if (!_typeChecker.CheckByTypeName(typeFullName))
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
            if (code == Code.Nop || ctx.Processed.Contains(instr))
                return FlowType.NextCycle;

            var method = ctx.Method;
            
            #region Awaiters in MoveNext as a boundary
            //find the approximate boundary between the business code
            //and the compiler-generated one
            if (method.SourceType.IsMoveNext && method.SourceType.MethodType == MethodType.CompilerGeneratedPart)
            {
                //TODO: this wrong!!! Нельзя тут прерывать, бизнес-код может быть ниже!!!
                if (code is Code.Callvirt or Code.Call)
                {
                    var s = instr.Previous?.ToString();
                    if (s.EndsWith("get_IsCompleted()"))
                        return FlowType.BreakCycle;
                }
                            
                //+margin if instruction starts the try/catch
                var delta = ctx.ExceptionHandlers.Any(a => a.TryStart == instr) ? 8 : 0;
                ctx.IncrementIndex(delta); 
            }
            #endregion

            return FlowType.NextOperand;
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
                            treeFunc.CalleeIndexes.Add(asyncMove.Fullname, ctx.SourceIndex);
                        }
                    }
                }
                if (!treeFunc.CalleeIndexes.ContainsKey(extFullname) && _typeChecker.CheckByMethodName(extFullname))
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
                        var mkey = GetMethodKey(realCgType.Fullname, extRealMethodName);
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

                    extType.FromMethod = treeFunc.Fullname;
                    //better process all methods, not filter only extName... may be... check it!
                    var extMethods = extType.Filter(typeof(InjectedMethod), true);
                    foreach (var injectedSimpleEntity in extMethods)
                    {
                        var meth = (InjectedMethod) injectedSimpleEntity;
                        meth.FromMethod = treeFunc.Fullname ?? extType.FromMethod;
                        if (meth.Name != extName) 
                            continue;
                        if (!treeFunc.CalleeIndexes.ContainsKey(meth.Fullname))
                            treeFunc.CalleeIndexes.Add(meth.Fullname, ctx.SourceIndex);
                    }
                }
                else
                {
                    if (!asmCtx.InjMethodByFullname.ContainsKey(extFullname)) 
                        return;
                    var extFunc = asmCtx.InjMethodByFullname[extFullname];
                    extFunc.FromMethod ??= treeFunc.Fullname ?? extType?.FromMethod;
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
                Log.Error(ex, $"Handling instruction: {ctx.ModuleName}; {ctx.Method.Fullname}; {nameof(ctx.CurIndex)}: {ctx.CurIndex}");
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
            string methodFullName = def.FullName;
            var type = def.DeclaringType;
            var declAttrs = type.CustomAttributes;
            var compGenAttrName = nameof(CompilerGeneratedAttribute);
            var isCompilerGeneratedType = def.Name.StartsWith("<") || 
                declAttrs.FirstOrDefault(a => a.AttributeType.Name == compGenAttrName) != null; 
            //                                                                                                          
            if (def.IsSetter)
                return MethodType.Setter;
            if (def.IsGetter)
                return MethodType.Getter;
            if (methodFullName.Contains("::add_"))
                return MethodType.EventAdd;
            if (methodFullName.Contains("::remove_"))
                return MethodType.EventRemove;
            if (isCompilerGeneratedType)
                return MethodType.CompilerGeneratedPart;
            //
            if (def.IsConstructor)
                return MethodType.Constructor;
            if (methodFullName.EndsWith("::Finalize()"))
                return MethodType.Destructor;
            if (methodFullName.Contains("|"))
                return MethodType.Local; 

            return MethodType.Normal;
        }

        internal bool IsSpecialGeneratedMethod(MethodType type)
        {
            return type is MethodType.EventAdd or MethodType.EventRemove;
        }

        internal List<MethodDefinition> GetMethods(TypeContext typeCtx, TypeDefinition type, MainOptions opts)
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
            treeParentClass.SourceType.IsAsyncStateMachine = interfaces
                .FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
            var isEnumerable = interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;
            var typeFullname = treeParentClass.Fullname;
            
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
                }
                #endregion
                
                var source = CreateMethodSource(ownMethod);
                var treeFunc = new InjectedMethod(treeParentClass.AssemblyName, typeFullname, 
                    treeParentClass.BusinessType, ownMethod.FullName, source);
                //
                var methodName = ownMethod.Name;
                source.IsMoveNext = methodName == "MoveNext";
                source.IsEnumeratorMoveNext = source.IsMoveNext && isEnumerable;
                source.IsFinalizer = methodName == "Finalize" && ownMethod.IsVirtual;
                if (source.MethodType == MethodType.CompilerGeneratedPart)
                    treeFunc.CompilerGeneratedInfo = new CalleeCodeBlock();
                //
                if (!asmCtx.InjMethodByFullname.ContainsKey(treeFunc.Fullname))
                    asmCtx.InjMethodByFullname.Add(treeFunc.Fullname, treeFunc);
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
                //     .Select(a => a.Fullname).ToList();
                // var func = funcs
                //     .FirstOrDefault(a => a == treeFunc.Fullname);
                
                treeParentClass.AddChild(treeFunc);
            }
            #endregion
            #region Nested classes
            foreach (var nestedType in type.NestedTypes)
            {
                var realTypeName = TryGetRealTypeName(nestedType);
                var treeType = new InjectedType(nestedType.Module.Name, nestedType.FullName, realTypeName)
                {
                    SourceType = CreateTypeSource(nestedType),
                };
                asmCtx.InjClasses.Add(treeType.Fullname, treeType);
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

        private ClassSource CreateTypeSource(TypeDefinition def)
        {
            return new ClassSource
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
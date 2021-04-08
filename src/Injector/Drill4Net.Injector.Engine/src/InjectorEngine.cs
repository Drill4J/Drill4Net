using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Globalization;
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
        private readonly ThreadLocal<AssemblyVersion> _mainVersion;
        private readonly HashSet<string> _restrictNamespaces;
        private Dictionary<string, InjectedType> _injClasses;
        private Dictionary<string, InjectedMethod> _injMethods;
        private Dictionary<string, InjectedMethod> _injMethodByClasses;
        protected ProbeHeper _probeHelper;
        private FlowStrategy _flowStrategy;

        /***************************************************************************************/

        public InjectorEngine( IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _isNetCore = new ThreadLocal<bool?>();
            _mainVersion = new ThreadLocal<AssemblyVersion>();
            _restrictNamespaces = GetRestrictNamespaces();
            _probeHelper = new ProbeHeper();
            _flowStrategy = new FlowStrategy();
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

        internal void ProcessDirectory( string directory,  Dictionary<string, AssemblyVersion> versions, 
             MainOptions opts,  InjectedSolution tree)
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
                AssemblyVersion version = null;
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

        private void ProcessAssembly(string filePath,  Dictionary<string, AssemblyVersion> versions,  
            MainOptions opts, InjectedSolution tree)
        {
            #region Reading
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");

            //filter
            var ns1 = Path.GetFileNameWithoutExtension(filePath).Split('.')[0];
            if (_restrictNamespaces.Contains(ns1))
                return;

            //source
            var sourceDir = $"{Path.GetFullPath(Path.GetDirectoryName(filePath))}\\";
            Environment.CurrentDirectory = sourceDir;
            var subjectName = Path.GetFileNameWithoutExtension(filePath);

            //destinaton
            var destDir = FileUtils.GetDestinationDirectory(opts, sourceDir);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            //must process? need to know the version of the assembly before reading it via cecil
            var ext = Path.GetExtension(filePath);
            var version = versions.ContainsKey(filePath) ? versions[filePath] : _rep.GetAssemblyVersion(filePath);
            if (version == null || (ext == ".exe" && version.Target == AssemblyVersionType.NetCore))
                return;

            #region Read params
            ReaderParameters readerParams = new ReaderParameters
            {
                // we will write to another file, so we don't need this
                ReadWrite = false,
                // read everything at once
                ReadingMode = ReadingMode.Immediate,
            };

            #region PDB
            var pdb = subjectName + ".pdb";
            var isPdbExists = File.Exists(pdb);
            var needPdb = isPdbExists && (version.Target == AssemblyVersionType.NetCore ||
                                          version.Target == AssemblyVersionType.NetStandard);
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
                    //log
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
            var targetVersionAtr = assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == typeof(System.Runtime.Versioning.TargetFrameworkAttribute).Name);
            string targetVersion = null;
            if (targetVersionAtr != null)
            {
                targetVersion = targetVersionAtr.ConstructorArguments[0].Value?.ToString();
                Console.WriteLine($"Version = {targetVersion}");
            }
            var targetFolder = ConvertTargetTypeToFolder(targetVersion);
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
            TypeReference proxyReturnTypeRef = module.TypeSystem.Void;
            var proxyNamespace = $"Injection_{Guid.NewGuid()}".Replace("-", null); //must be unique for each target asm
            var proxyTypeRef = new TypeReference(proxyNamespace, opts.Proxy.Class, module, module);
            var proxyMethRef = new MethodReference(opts.Proxy.Method, proxyReturnTypeRef, proxyTypeRef);
            var strPar = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            proxyMethRef.Parameters.Add(strPar);

            // 2. 'Call' command
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);
            #endregion
            #region Processing
            _injClasses = new Dictionary<string, InjectedType>();
            _injMethods = new Dictionary<string, InjectedMethod>();
            _injMethodByClasses = new Dictionary<string, InjectedMethod>();

            Instruction lastOp;
            var probData = string.Empty;

            foreach (TypeDefinition typeDef in module.Types)
            {
                var typeName = typeDef.Name;
                if (typeName == "<Module>")
                    continue;

                //TODO: normal defining of business types (by cfg?)
                var nameSpace = typeDef.Namespace;
                var typeFullName = typeDef.FullName;
                var tAr = typeFullName.Split('.');
                ns1 = tAr[0];
                if (_restrictNamespaces.Contains(ns1)) //Guanito
                    continue;

                #region Tree
                var realTypeName = TryGetRealTypeName(typeDef);
                var treeType = new InjectedType(treeAsm.Name, typeFullName, realTypeName)
                {
                    SourceType = CreateTypeSource(typeDef)
                };
                _injClasses.Add(treeType.Fullname, treeType);
                treeAsm.AddChild(treeType);
                #endregion

                //collect methods including business & compiler's nested classes
                //together (for async, delegates, anonymous types...)
                var methods = GetAllMethods(treeType, typeDef, opts);

                //process all methods
                foreach (var methodDef in methods)
                {
                    #region Init
                    var curType = methodDef.DeclaringType;
                    typeName = curType.FullName;
                    typeFullName = curType.FullName;

                    var methodName = methodDef.Name;
                    var methodFullName = methodDef.FullName;

                    //Tree
                    treeType = _injClasses[typeFullName];
                    var typeSource = treeType.SourceType;
                    var treeFunc = _injMethods[methodFullName];
                    var methodSource = treeFunc.SourceType;
                    var methodType = methodSource.MethodType;

                    //the CompilerGeneratedAttribute itself is not enough!
                    //not use isMoveNext - this class may be own iterator, not compirer's one
                    var isCompilerGenerated = methodType == MethodType.CompilerGenerated;

                    var isAsyncStateMachine = typeSource.IsAsyncStateMachine;
                    var skipStart = isAsyncStateMachine || methodSource.IsEnumeratorMoveNext; //skip state machine init jump block, etc

                    //instructions
                    var body = methodDef.Body;
                    //body.SimplifyMacros(); //buggy (Cecil or me?)
                    var instructions = body.Instructions; //no copy list!
                    var processor = body.GetILProcessor();
                    #endregion
                    #region Enter/Return
                    //inject 'entering' instruction
                    var isSpecFunc = IsSpecialGeneratedMethod(methodType);
                    var strictEnterReturn = //what is principally forbidden
                        !isSpecFunc &&
                        (                           
                            methodName.Contains("|") || //local func                                                        
                            isAsyncStateMachine || //async/await
                            isCompilerGenerated ||
                            //Finalyze() -> strange, but for Core 'Enter' & 'Return' lead to a crash                   
                            (_isNetCore.Value == true && methodSource.IsFinalizer)
                        );

                    Instruction ldstrEntering = null;
                    if (!strictEnterReturn)
                    {
                        probData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.Enter, 0);
                        ldstrEntering = GetInstruction(probData);

                        var firstOp = instructions.First();
                        processor.InsertBefore(firstOp, ldstrEntering);
                        processor.InsertBefore(firstOp, call);
                    }

                    //return
                    var returnProbData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.Return, -1);
                    var ldstrReturn = GetInstruction(probData); //as object it must be only one
                    lastOp = instructions.Last();
                    #endregion
                    #region Context
                    var ctx = new InjectorContext(moduleName, methodFullName, instructions, processor)
                    {
                        TreeType = treeType,
                        TreeMethod = treeFunc,
                        IsStrictEnterReturn = strictEnterReturn,
                        LastOperation = lastOp,
                        LdstrReturn = ldstrReturn,
                        ProxyMethRef = proxyMethRef,
                        ReturnProbData = returnProbData,
                        ExceptionHandlers = body.ExceptionHandlers,
                    };
                    #endregion
                    #region Jumps
                    //collect jumps. Hash table for separate addresses is almost useless,
                    //because they may be recalculated inside the processor during inject...
                    //and ideally, there shouldn't be too many of them 
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var flow = instr.OpCode.FlowControl;
                        if (flow == FlowControl.Branch || flow == FlowControl.Cond_Branch)
                            ctx.Jumpers.Add(instr);
                    }
                    #endregion
                    #region Injections               
                    var startInd = skipStart ? 12 : 1;
                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var opCode = instr.OpCode;
                        var code = opCode.Code;
                        var flow = opCode.FlowControl;

                        MapBusinessFunction(instr, treeFunc);

                        #region Checks
                        // for injecting cases
                        if (ctx.Processed.Contains(instr))
                            continue;

                        #region Awaiters in MoveNext as a boundary
                        //we need check: do now current code part is business part
                        //or is compiler generated part?
                        if (methodSource.IsMoveNext && isCompilerGenerated)
                        {
                            //TODO: caching!!!
                            foreach (var catcher in body.ExceptionHandlers)
                            {
                                if (catcher.TryStart == instr)
                                {
                                    i += 8;
                                    continue;
                                }
                            }

                            if (code == Code.Callvirt || code == Code.Call)
                            {
                                var s = instr.ToString();
                                if (s.EndsWith("get_IsCompleted()")) 
                                    break;
                            }
                        }
                        #endregion
                        #endregion

                        //the return in the middle of the method body
                        if (instr.Operand == lastOp && !strictEnterReturn && lastOp.OpCode.Code != Code.Endfinally) //jump to the end for return from function
                        {
                            ldstrReturn.Operand = $"{returnProbData}{i}";
                            instr.Operand = ldstrReturn;
                        }

                        //main processing
                        ctx.SetIndex(i);
                        HandleInstructionInContext(ctx);
                        i = ctx.CurIndex;
                    }
                    #endregion
                    #region Correct jumps
                    //EACH short form -> to long form (otherwise, you need to recalculate 
                    //again after each necessary conversion)
                    var jumpersList = ctx.Jumpers.ToList();
                    for (var j = 0; j < jumpersList.Count; j++)
                    {
                        var jump = jumpersList[j];
                        var opCode = jump.OpCode;
                        if (jump.Operand is Instruction operand)
                        {
                            var newOpCode = ConvertShortJumpToLong(opCode);
                            if (newOpCode.Code != opCode.Code)
                                jump.OpCode = newOpCode;
                        }
                    }
                    #endregion

                    body.Optimize();
                    body.OptimizeMacros();
                }
            }
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
        internal protected OpCode ConvertShortJumpToLong(OpCode opCode)
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

        internal void MapBusinessFunction(Instruction instr, InjectedMethod treeFunc)
        {
            var flow = instr.OpCode.FlowControl;
            var code = instr.OpCode.Code;

            //in any case needed check compiler generated classes
            if (instr.Operand is MethodReference &&
               (flow == FlowControl.Call || code == Code.Ldftn || code == Code.Ldfld))
            {
                //TODO: cache!
                var extOp = instr.Operand as MethodReference;
                var extTypeFullName = extOp.DeclaringType.FullName;
                var extTypeName = extOp.DeclaringType.Name;
                var extFullname = extOp.FullName;
                var extName = extOp.Name;
                if (extName.StartsWith("<") || extTypeFullName.Contains(">d__") || extFullname.Contains("|"))
                {
                    try
                    {
                        //null is norm for anonymous types
                        var extType = _injClasses.ContainsKey(extTypeFullName) ?
                                          _injClasses[extTypeFullName] :
                                          (_injClasses.ContainsKey(extTypeName) ? _injClasses[extTypeName] : null);

                        //extType found, not local func, not 'class-for-all'
                        if (!extFullname.Contains("|") && extType?.Name?.EndsWith("/<>c") == false)
                        {
                            var extRealMethodName = TryGetBusinessMethod(extFullname, extFullname, true, true);
                            InjectedType realCgType = null;
                            var typeKey = treeFunc.BusinessType; // realTypeName ?? typeFullName
                            if (_injClasses.ContainsKey(typeKey))
                                realCgType = _injClasses[typeKey];
                            var methodKey = extRealMethodName; // ?? realMethodName;
                            if (realCgType != null && methodKey != null)
                            {
                                var mkey = GetMethodKeyByClass(realCgType.Fullname, methodKey);
                                if (_injMethodByClasses.ContainsKey(mkey))
                                    treeFunc = _injMethodByClasses[mkey];
                                var nestTypes = extType.Filter(typeof(InjectedType), true);
                                foreach (InjectedType nestType in nestTypes)
                                {
                                    if (nestType.IsCompilerGenerated)
                                        nestType.FromMethod = treeFunc.FromMethod ?? treeFunc.Fullname;
                                }
                            }
                            if (extType != null)
                            {
                                extType.FromMethod = treeFunc.Fullname;
                                var extMethods = extType.Filter(typeof(InjectedMethod), true);
                                foreach (InjectedMethod meth in extMethods)
                                {
                                    meth.FromMethod = treeFunc.Fullname ?? extType.FromMethod;
                                }
                            }
                        }
                        else
                        {
                            if (_injMethods.ContainsKey(extFullname))
                            {
                                var extFunc = _injMethods[extFullname];
                                if (extFunc.FromMethod == null)
                                    extFunc.FromMethod = treeFunc.Fullname ?? extType.FromMethod;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Getting real name of func method: [{extOp}]");
                    }
                }
            }
        }

        internal void HandleInstructionInContext(InjectorContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            //
            var initIndex = ctx.CurIndex;
            try
            {
                _flowStrategy.Handle(ctx);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Handle od instruction: {ctx.ModuleName}; {ctx.MethodFullName}; index: {initIndex}");
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
            string realMethodName = null;
            if (isCompilerGenerated || isAsyncStateMachine)
            {
                try
                {
                    if (methodName.Contains("|")) //local funcs
                    {
                        var a1 = methodName.Split('>')[0];
                        return a1.Substring(1, a1.Length-1);
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
                        else
                        if (fromMethodName)
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
                catch
                { }
            }
            return realMethodName;
        }

        internal string TryGetRealTypeName(TypeDefinition type)
        {
            if (type?.DeclaringType != null)
            {
                do type = type.DeclaringType;
                    while (type.DeclaringType != null && (type.Name.Contains("c__DisplayClass") || type.Name.Contains("<>")));
            }
            return type?.FullName;
        }

        internal MethodType GetMethodType(MethodDefinition def)
        {
            string methodFullName = def.FullName;
            var type = def.DeclaringType;
            var declAttrs = type.CustomAttributes;
            var compGenAttrName = typeof(CompilerGeneratedAttribute).Name;
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
                return MethodType.CompilerGenerated;
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
            return type == MethodType.EventAdd || type == MethodType.EventRemove;
        }

        internal List<MethodDefinition> GetAllMethods(InjectedType treeParentClass, TypeDefinition type, 
            MainOptions opts)
        {
            var methods = new List<MethodDefinition>();
            var typeFullname = treeParentClass.Fullname;

            #region Own methods
            var probOpts = opts.Probes;
            var isAngleBracket = type.Name.StartsWith("<");

            //check for async/await
            var interfaces = type.Interfaces;
            treeParentClass.SourceType.IsAsyncStateMachine = interfaces
                .FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
            var isEnumerable = interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;

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

            foreach (var ownMethod in ownMethods)
            {
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
                //
                var methodSource = CreateMethodSource(ownMethod);
                var treeFunc = new InjectedMethod(typeFullname, treeParentClass.BusinessType, ownMethod.FullName, methodSource);
                //
                var methodName = ownMethod.Name;
                methodSource.IsMoveNext = methodName == "MoveNext";
                methodSource.IsEnumeratorMoveNext = methodSource.IsMoveNext && isEnumerable;
                methodSource.IsFinalizer = methodName == "Finalize" && ownMethod.IsVirtual;
                //
                if (!_injMethods.ContainsKey(treeFunc.Fullname))
                    _injMethods.Add(treeFunc.Fullname, treeFunc);
                else { } //strange..
                methods.Add(ownMethod);
                //
                var method = GetMethodKeyByClass(typeFullname, treeFunc.Name);
                if (!_injMethodByClasses.ContainsKey(method))
                    _injMethodByClasses.Add(method, treeFunc);
                else { }
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
                _injClasses.Add(treeType.Fullname, treeType);
                treeParentClass.AddChild(treeType);
                //
                var innerMethods = GetAllMethods(treeType, nestedType, opts);
                methods.AddRange(innerMethods);
            }
            #endregion

            methods = methods
                .Where(a => a.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == typeof(DebuggerHiddenAttribute).Name) == null)
                .ToList();
            return methods;
        }

        private string GetMethodKeyByClass(string typeFullname, string methodShortName)
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

        private Instruction GetInstruction(string probeData)
        {
            return Instruction.Create(OpCodes.Ldstr, probeData);
        }

        private HashSet<string> GetRestrictNamespaces()
        {
            var hash = new HashSet<string>
            {
                "Microsoft",
                "Windows",
                "System",
                "FSharp"
                //...
            };
            return hash;
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
            var dirs = tree.GetAllDirectories();
            if (dirs.Any())
            {
                var pathInText = _rep.GetTreeFilePath(tree);
                Log.Debug($"Tree saved to: [{pathInText}]");
                foreach (var dir in dirs)
                {
                    var hintPath = _rep.GetTreeFileHintPath(dir.DestinationPath);
                    File.WriteAllText(hintPath, pathInText);
                    Log.Debug($"Hint placed to: [{hintPath}]");
                }
            }
        }
        #endregion
    }
}
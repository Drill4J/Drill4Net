using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Drill4Net.Injector.Core;

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
        private readonly EnumerationOptions _searchOpts;
        private readonly HashSet<string> _restrictNamespaces;
        private int _curPointUid;

        /***************************************************************************************/

        public InjectorEngine([NotNull] IInjectorRepository rep)
        {
            _rep = rep;
            _isNetCore = new ThreadLocal<bool?>();
            _mainVersion = new ThreadLocal<AssemblyVersion>();
            _searchOpts = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false,
                RecurseSubdirectories = false
            };
            _restrictNamespaces = GetRestrictNamespaces();
        }

        /***************************************************************************************/

        public InjectedSolution Process()
        {
            return Process(_rep.Options);
        }

        public InjectedSolution Process([NotNull] MainOptions opts)
        {
            _rep.ValidateOptions(opts);
            CopySource(opts.Source.Directory, opts.Destination.Directory);
            var versions = DefineTargetVersions(opts.Source.Directory);
            _curPointUid = -1;
            //
            var dir = opts.Source.Directory;
            var tree = new InjectedSolution(opts.Target?.Name, dir)
            {
                StartTime = DateTime.Now,
                DestinationPath = GetDestinationDirectory(opts, dir),
            };
            ProcessDirectory(dir, versions, opts, tree);
            tree.FinishTime = DateTime.Now;
            InjectTree(tree);
            //
            return tree;
        }

        internal void ProcessDirectory([NotNull] string directory, [NotNull] Dictionary<string, AssemblyVersion> versions, 
            [NotNull] MainOptions opts, [NotNull] InjectedSolution tree)
        {
            //files
            var files = IoHelper.GetAssemblies(directory, _searchOpts);
            foreach (var file in files)
            {
                ProcessAssembly(file, versions, opts, tree);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*", _searchOpts);
            foreach (var dir in dirs)
            {
                ProcessDirectory(dir, versions, opts, tree);
            }
        }

        internal Dictionary<string, AssemblyVersion> DefineTargetVersions([NotNull] string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Source directory not exists: [{directory}]");
            var files = IoHelper.GetAssemblies(directory, _searchOpts);
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
                version = GetAssemblyVersion(file);
                versions.Add(file, version);
                //
                if (_isNetCore.Value == null)
                {
                    if (version.Target == AssemblyVersionType.NetCore)
                    {
                        _mainVersion.Value = version;
                        _isNetCore.Value = true;
                    }
                    if (version.Target == AssemblyVersionType.NetFramework)
                    {
                        _mainVersion.Value = version;
                        _isNetCore.Value = false;
                    }
                }
            }
            return versions;
        }

        internal void ProcessAssembly([NotNull] string filePath, [NotNull] Dictionary<string, 
            AssemblyVersion> versions, [NotNull] MainOptions opts, [NotNull] InjectedSolution tree)
        {
            #region Reading
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");

            //filter
            var ns1 = Path.GetFileNameWithoutExtension(filePath).Split(".")[0];
            if (_restrictNamespaces.Contains(ns1))
                return;

            //source
            var sourceDir = $"{Path.GetFullPath(Path.GetDirectoryName(filePath))}\\";
            Environment.CurrentDirectory = sourceDir;
            var subjectName = Path.GetFileNameWithoutExtension(filePath);

            //destinaton
            string destDir = GetDestinationDirectory(opts, sourceDir);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            //must process?
            var ext = Path.GetExtension(filePath);
            var version = versions.ContainsKey(filePath) ? versions[filePath] : GetAssemblyVersion(filePath);
            if (ext == ".exe" && version.Target == AssemblyVersionType.NetCore)
                return;

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

            // read subject assembly with symbols
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filePath, readerParams);
            var module = assembly.MainModule;

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
            #endregion
            #region Commands

            // 1. Command ref

            //we will use proxy class (with cached Reflection) leading to real profiler
            //proxy will be inject in each target assembly - let construct the calling of it's method
            TypeReference proxyReturnTypeRef = module.TypeSystem.Void;
            var proxyNamespace = $"Injection_{Guid.NewGuid()}".Replace("-", null); //must be unique for each target asm
            var proxyTypeRef = new TypeReference(proxyNamespace, opts.Proxy.Class, module, module);
            var proxyMethRef = new MethodReference(opts.Proxy.Method, proxyReturnTypeRef, proxyTypeRef);
            var strPar = new ParameterDefinition("data", Mono.Cecil.ParameterAttributes.None, module.TypeSystem.String);
            proxyMethRef.Parameters.Add(strPar);

            // 2. 'Call' command
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);
            #endregion
            #region Processing
            HashSet<Instruction> jumpers;
            Mono.Collections.Generic.Collection<Instruction> instructions;
            HashSet<Instruction> compilerInstructions;
            var dbgHiddenAttrName = typeof(DebuggerHiddenAttribute).Name;
            bool isAsyncStateMachine = false;
            Instruction lastOp;

            foreach (TypeDefinition typeDef in module.Types)
            {
                var typeName = typeDef.Name;
                //TODO: normal defining of business types (by cfg?)
                if (typeName == "<Module>" || typeName.StartsWith("Microsoft.") || typeName.StartsWith("System.")) //GUANO....
                    continue;
                var nameSpace = typeDef.Namespace;
                var typeFullName = typeDef.FullName;

                #region Tree
                var treeClass = new InjectedClass(treeAsm.Name, typeFullName)
                {
                    Source = CreateTypeSource(typeDef)
                };
                treeAsm.AddChild(treeClass);
                #endregion

                //collect methods including business & compiler's nested classes (for async, delegates, anonymous types...)
                var methods = GetAllMethods(typeDef, opts);

                //process all methods
                foreach (var methodDef in methods)
                {
                    #region Init
                    var curType = methodDef.DeclaringType;
                    typeName = curType.FullName;

                    var methodName = methodDef.Name;
                    var methodFullName = methodDef.FullName;

                    var isMoveNext = methodName == "MoveNext";
                    var isFinalizer = methodName == "Finalize" && methodDef.IsVirtual;
                    var moduleName = module.Name;
                    var probData = string.Empty;
                    HashSet<Instruction> _processed = new HashSet<Instruction>();

                    //instructions
                    var body = methodDef.Body;
                    //body.SimplifyMacros(); //buggy (Cecil or me?)
                    instructions = body.Instructions; //no copy list!
                    var processor = body.GetILProcessor();
                    compilerInstructions = new HashSet<Instruction>();
                    var startInd = 1;

                    //method's attributes
                    var methAttrs = methodDef.CustomAttributes;
                    var isDbgHidden = methAttrs.FirstOrDefault(a => a.AttributeType.Name == dbgHiddenAttrName) != null;
                    if (isDbgHidden)
                        continue;

                    //check for async/await
                    var interfaces = curType.Interfaces;
                    isAsyncStateMachine = interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
                    var isEnumeratorMoveNext = isMoveNext && interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IEnumerable") != null;
                    var skipStart = isAsyncStateMachine || isEnumeratorMoveNext;  //skip state machine init jump block, etc
                    if (skipStart)
                        startInd = 12;

                    var methodType = GetMethodType(methodDef);
                    var isCompilerGenerated = methodType == MethodType.CompilerGenerated;

                    #region Tree
                    var treeFunc = new InjectedMethod(nameSpace, methodName);
                    var isNested = false; //TODO: DEFINE & FIX !!!
                    var methHashcode = GetMethodHashCode(body.Instructions);
                    treeFunc.Source = CreateMethodSource(methodDef, methodType, isNested, methHashcode);
                    treeClass.AddChild(treeFunc);
                    #endregion
                    #region Real type & method names
                    //TODO: regex!!!
                    TypeDefinition realType = null;
                    string realTypeName = null;
                    string realMethodName = null;
                    //var realMethodFullmame = methodFullName;
                    if (isCompilerGenerated || isAsyncStateMachine)
                    {
                        try
                        {
                            var fromMethodName = typeName.Contains("c__DisplayClass") || typeName.Contains("<>");
                            if (isMoveNext || !fromMethodName && typeName.Contains("/"))
                            {
                                var ar = typeName.Split("/");
                                var el = ar[ar.Length - 1];
                                realMethodName = el.Split(">")[0].Replace("<", null);
                            }
                            else
                            if (fromMethodName)
                            {
                                var tmp = methodName.Replace("<>", null);
                                if(tmp.Contains("<"))
                                    realMethodName = tmp.Split("<")[1].Split(">")[0];
                            }
                        }
                        catch(Exception ex) 
                        { }

                        if (curType.DeclaringType != null)
                        {
                            realType = curType;
                            do realType = realType.DeclaringType;
                                while (realType.DeclaringType != null && (realType.Name.Contains("c__DisplayClass") || realType.Name.Contains("<>")));
                        }
                        realTypeName = realType?.FullName;
                    }
                    #endregion
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
                            //Finalyze() -> strange, but for Core 'Enter' & 'Leaving' lead to a crash                   
                            (_isNetCore.Value == true && isFinalizer)
                        );

                    Instruction ldstrEntering = null;
                    if (!strictEnterReturn)
                    {
                        probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, CrossPointType.Enter, 0);
                        ldstrEntering = GetInstruction(probData);

                        var firstOp = instructions.First();
                        processor.InsertBefore(firstOp, ldstrEntering);
                        processor.InsertBefore(firstOp, call);
                    }

                    //return
                    var returnProbData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, CrossPointType.Return, -1);
                    var ldstrReturn = GetInstruction(probData); //as object it must be only one
                    lastOp = instructions.Last();
                    #endregion
                    #region Jumps
                    //collect jumps. Hash table for addresses is almost useless,
                    //because they may be recalculated inside the processor during inject...
                    //and ideally, there shouldn't be too many of them 
                    jumpers = new HashSet<Instruction>();
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var flow = instr.OpCode.FlowControl;
                        if (flow == FlowControl.Branch || flow == FlowControl.Cond_Branch)
                            jumpers.Add(instr);
                    }
                    #endregion
                    #region Injections               
                    var ifStack = new Stack<Instruction>();
                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        if (_processed.Contains(instr))
                            continue;
                        var opCode = instr.OpCode;
                        var code = opCode.Code;
                        var flow = opCode.FlowControl;
                        CrossPointType crossType = CrossPointType.Unset;

                        //awaiters in MoveNext as a border
                        if (isMoveNext && isCompilerGenerated)
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

                        if (instr.Operand == lastOp && !strictEnterReturn && lastOp.OpCode.Code != Code.Endfinally) //jump to the end for return from function
                        {
                            ldstrReturn.Operand = $"{returnProbData}{i}";
                            instr.Operand = ldstrReturn;
                        }

                        // IF/SWITCH
                        if (flow == FlowControl.Cond_Branch)
                        {
                            //check for 'using' statement (compiler generated Try/Finally with If-checking)
                            //There is a possibility that a very similar construction from the business code
                            //may be omitted if the programmer directly implemented Try/Finally with a check
                            //and a Dispose() call, instead of the usual 'using', although it is unlikely
                            if (i > 2 && i < instructions.Count - 2)
                            {
                                var prev2 = instructions[i - 2].OpCode.Code;
                                if (prev2 == Code.Throw)
                                    continue;
                                var isWasTry = prev2 == Code.Leave || prev2 == Code.Leave_S;
                                if (isWasTry)
                                {
                                    var b = instructions[i + 2];
                                    var isDispose = (b.Operand as MemberReference)?.FullName?.EndsWith("IDisposable::Dispose()") == true;
                                    if (isDispose)
                                        continue;
                                }
                            }
                            //
                            if (!isAsyncStateMachine && !isEnumeratorMoveNext && IsCompilerGeneratedBranch(i))
                                continue;
                            if (!IsRealCondition(i))
                                continue;
                            //
                            var isBrFalse = code == Code.Brfalse || code == Code.Brfalse_S; //TODO: add another branch codes? Hmm...

                            //lock/Monitor
                            var operand = instr.Operand as Instruction;
                            if (isBrFalse && operand != null && operand.OpCode.Code == Code.Endfinally)
                            {
                                var endFinInd = instructions.IndexOf(operand);
                                var prevInstr = SkipNop(endFinInd, false);
                                var operand2 = prevInstr.Operand as MemberReference;
                                if (operand2?.FullName?.Equals("System.Void System.Threading.Monitor::Exit(System.Object)") == true)
                                    continue;
                            }

                            //operators: while/do
                            if (operand != null && operand.Offset > 0 && instr.Offset > operand.Offset)
                            {
                                if (isEnumeratorMoveNext)
                                {
                                    var prevRef = (instr.Previous.Operand as MemberReference).FullName;
                                    if (prevRef?.Contains("::MoveNext()") == true)
                                        continue;
                                }
                                //
                                var ind = instructions.IndexOf(operand); //inefficient, but it will be rarely...
                                var prevOperand = SkipNop(ind, false);
                                if (prevOperand.OpCode.Code == Code.Br || prevOperand.OpCode.Code == Code.Br_S) //while
                                {
                                    probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, CrossPointType.While, i);
                                    var ldstrIf2 = GetInstruction(probData);
                                    var targetOp = prevOperand.Operand as Instruction;
                                    processor.InsertBefore(targetOp, ldstrIf2);
                                    processor.InsertBefore(targetOp, call);
                                    i += 2;
                                }
                                else //do
                                {
                                    //no signaling...
                                }
                                continue;
                            }

                            // switch
                            ifStack.Push(instr);
                            if (code == Code.Switch)
                            {
                                for (var k = 0; k < ((Instruction[])instr.Operand).Length - 1; k++)
                                    ifStack.Push(instr);
                                crossType = CrossPointType.Switch;
                            }

                            // IF
                            if (code == Code.Switch || instructions[i + 1].OpCode.FlowControl != FlowControl.Branch) //empty IF?
                            {
                                if (crossType == CrossPointType.Unset)
                                    crossType = isBrFalse ? CrossPointType.If : CrossPointType.Else;
                                probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, crossType, i);
                                var ldstrIf = GetInstruction(probData);

                                //when inserting 'after', must set in desc order
                                processor.InsertAfter(instr, call);
                                processor.InsertAfter(instr, ldstrIf);
                                i += 2;
                            }

                            //for 'switch when()', etc
                            var prev = operand?.Previous;
                            if (prev == null || _processed.Contains(prev))
                                continue;
                            var prevCode = prev.OpCode.Code;
                            if (prevCode == Code.Br || prevCode == Code.Br_S) //need insert paired call
                            {
                                //TODO: совместить с веткой ELSE/JUMP ?
                                crossType = crossType == CrossPointType.If ? CrossPointType.Else : CrossPointType.If;
                                var ind = instructions.IndexOf(operand);
                                probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, crossType, ind);
                                var elseInst = GetInstruction(probData);

                                ReplaceJump(operand, elseInst);
                                processor.InsertBefore(operand, elseInst);
                                processor.InsertBefore(operand, call);

                                _processed.Add(prev);
                                if (operand.Offset < instr.Offset)
                                    i += 2;
                            }
                            //
                            continue;
                        }

                        // ELSE/JUMP
                        if (flow == FlowControl.Branch && (code == Code.Br || code == Code.Br_S))
                        {
                            if (!ifStack.Any())
                                continue;
                            if (IsNextReturn(i))
                                continue;
                            if (!isAsyncStateMachine && IsCompilerGeneratedBranch(i))
                                continue;
                            if (!IsRealCondition(i)) //is real condition's branch?
                                continue;
                            //
                            var ifInst = ifStack.Pop();
                            var pairedCode = ifInst.OpCode.Code;
                            crossType = pairedCode == Code.Brfalse || pairedCode == Code.Brfalse_S ? CrossPointType.Else : CrossPointType.If;
                            probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, crossType, i);
                            var elseInst = GetInstruction(probData);

                            var instr2 = instructions[i + 1];
                            FixFinallyEnd(instr, elseInst, body.ExceptionHandlers);
                            ReplaceJump(instr2, elseInst);
                            processor.InsertBefore(instr2, elseInst);
                            processor.InsertBefore(instr2, call);
                            i += 2;
                            continue;
                        }

                        //THROW
                        if (flow == FlowControl.Throw)
                        {
                            probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, CrossPointType.Throw, i);
                            var throwInst = GetInstruction(probData);
                            FixFinallyEnd(instr, throwInst, body.ExceptionHandlers);
                            ReplaceJump(instr, throwInst);
                            processor.InsertBefore(instr, throwInst);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }

                        //CATCH FILTER
                        if (code == Code.Endfilter)
                        {
                            probData = GetProbeData(treeFunc, methodDef, moduleName, realMethodName, methodFullName, CrossPointType.CatchFilter, i);
                            var ldstrFlt = GetInstruction(probData);
                            FixFinallyEnd(instr, ldstrFlt, body.ExceptionHandlers);
                            //ReplaceJump(instr, ldstrReturn);
                            processor.InsertBefore(instr, ldstrFlt);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }

                        //RETURN
                        if (code == Code.Ret && !strictEnterReturn)
                        {
                            ldstrReturn.Operand = $"{returnProbData}{i}";
                            FixFinallyEnd(instr, ldstrReturn, body.ExceptionHandlers);
                            ReplaceJump(instr, ldstrReturn);
                            processor.InsertBefore(instr, ldstrReturn);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }
                    
                    } //cycle
                    #endregion
                    #region Correct jumps
                    if (skipStart)
                    {
                        //EACH short form -> to long form (otherwise, you need to recalculate 
                        //again after each necessary conversion)
                        var jumpersList = jumpers.ToList();
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
                    }
                    #endregion

                    body.Optimize();
                    body.OptimizeMacros();
                }
            }

            // ensure we referencing only ref assemblies
            var systemPrivateCoreLib = module.AssemblyReferences.FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
            if (systemPrivateCoreLib != null)
                module.AssemblyReferences.Remove(systemPrivateCoreLib);
            //Debug.Assert(systemPrivateCoreLib == null, "systemPrivateCoreLib == null");
            #endregion
            #region Proxy class
            //here we generate proxy class which will be calling of real profiler by cached Reflection
            //directory of profiler dependencies - for injected target on it's side
            var profilerOpts = opts.Profiler;
            var proxyGenerator = new ProfilerProxyGenerator(proxyNamespace, opts.Proxy.Class, opts.Proxy.Method, //proxy to profiler
                                                            profilerOpts.Directory, profilerOpts.AssemblyName, //real profiler
                                                            profilerOpts.Namespace, profilerOpts.Class, profilerOpts.Method);
            proxyGenerator.InjectTo(assembly);
            #endregion
            #region Saving
            Environment.CurrentDirectory = destDir;
            var modifiedPath = $"{Path.Combine(destDir, subjectName)}{ext}";

            // save modified assembly and symbols to new file    
            var writeParams = new WriterParameters();
            if (needPdb)
            {
                writeParams.SymbolStream = File.Create(pdb);
                writeParams.WriteSymbols = true;
                // net core uses portable pdb
                writeParams.SymbolWriterProvider = new PortablePdbWriterProvider();
            }
            assembly.Write(modifiedPath, writeParams);
            assembly.Dispose();
            #endregion

            Console.WriteLine($"Modified assembly is created: {modifiedPath}");

            #region Local functions

            void FixFinallyEnd(Instruction cur, Instruction on, Mono.Collections.Generic.Collection<ExceptionHandler> handlers)
            {
                var prev = cur.Previous;
                var prevCode = prev.OpCode.Code;
                if (prevCode == Code.Endfinally)
                {
                    foreach (var exc in handlers)
                    {
                        if (exc.HandlerEnd == cur)
                            exc.HandlerEnd = on;
                    }
                }
            }

            OpCode ConvertShortJumpToLong(OpCode opCode)
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

            bool IsRealCondition(int ind)
            {
                if (ind < 0 || ind >= instructions.Count)
                    return false;
                //
                var op = instructions[ind];
                if (isAsyncStateMachine)
                {
                    var prev = SkipNop(ind, false);
                    var prevOpS = prev.Operand?.ToString();
                    var isInternal = prev.OpCode.Code == Code.Call &&
                                        prevOpS != null &&
                                        (prevOpS.EndsWith("TaskAwaiter::get_IsCompleted()") || prevOpS.Contains("TaskAwaiter`1"))
                                        ;
                    if (isInternal)
                        return false;

                    //seems not good: skip some state machine's instructions

                    // machine state not init jump block (yeah...)
                    if (instructions[ind - 1].OpCode.Code == Code.Ldloc_0 &&
                        instructions[ind + 1].OpCode.FlowControl == FlowControl.Branch &&
                        instructions[ind + 2].OpCode.FlowControl == FlowControl.Branch &&
                        instructions[ind + 3].OpCode.Code == Code.Nop &&
                        op.Operand == instructions[ind + 2] &&
                        instructions[ind + 1].Operand == instructions[ind + 3]
                        )
                        return false;

                    //1. start of finally block of state machine
                    if (op.OpCode.Code == Code.Bge_S &&
                        instructions[ind - 1].OpCode.Code == Code.Ldc_I4_0 &&
                        instructions[ind - 2].OpCode.Code == Code.Ldloc_0 &&
                        instructions[ind - 3].OpCode.Code == Code.Leave_S
                        )
                        return false;

                    //2. end of finally block of state machine
                    if (op.OpCode.Code == Code.Brfalse_S &&
                        instructions[ind + 1].OpCode.Code == Code.Ldarg_0 &&
                        instructions[ind + 2].OpCode.Code == Code.Ldfld &&
                        instructions[ind + 3].OpCode.Code == Code.Callvirt &&
                        instructions[ind + 4].OpCode.Code == Code.Nop &&
                        instructions[ind + 5].OpCode.Code == Code.Endfinally
                        )
                        return false;
                }
                //
                var next = SkipNop(ind, true);

                return op.Operand != next; //how far do it jump?
            }

            Instruction SkipNop(int ind, bool forward)
            {
                int start, end, inc;
                if (forward)
                {
                    start = ind + 1;
                    end = instructions.Count - 1;
                    inc = 1;
                }
                else
                {
                    start = ind - 1;
                    end = 0;
                    inc = -1;
                }
                //
                for (var i = start; true; i += inc)
                {
                    if (i >= instructions.Count || i < 0)
                        break;
                    var op = instructions[i];
                    if (op.OpCode.Code == Code.Nop)
                        continue;
                    return op;
                }
                return Instruction.Create(OpCodes.Nop);
            }

            void ReplaceJump(Instruction from, Instruction to)
            {
                //direct jumps
                foreach (var curOp in jumpers.Where(j => j.Operand == from))
                    curOp.Operand = to;

                //switches
                foreach (var curOp in jumpers.Where(j => j.OpCode.Code == Code.Switch))
                {
                    var switches = (Instruction[])curOp.Operand;
                    for (int i = 0; i < switches.Length; i++)
                    {
                        if (switches[i] == from)
                            switches[i] = to;
                    }
                }
            }

            bool IsCompilerGeneratedBranch(int ind)
            {
                //TODO: optimize (caching 'normal instruction')
                if (ind < 0 || ind >= instructions.Count)
                    return false;
                Instruction instr = instructions[ind];
                if (instr.OpCode.FlowControl != FlowControl.Cond_Branch)
                    return false;
                //
                Instruction inited = instr;
                Instruction finish = inited.Operand as Instruction;
                var localInsts = new List<Instruction>();

                while (true)
                {
                    if (instr == null || instr.Offset == 0)
                        break;

                    //we don't need compiler generated instructions in business code
                    if (!compilerInstructions.Contains(instr))
                    {
                        localInsts.Add(instr);
                        var operand = instr.Operand as MemberReference;
                        if (operand?.Name.StartsWith("<") == true) //hm...
                        {
                            //add next instructions of branch as 'angled'
                            var curNext = inited.Next;
                            while (true)
                            {
                                var flow = curNext.OpCode.FlowControl;
                                if (curNext == null || curNext == finish || flow == FlowControl.Return || flow == FlowControl.Throw)
                                    break;
                                localInsts.Add(curNext);
                                curNext = curNext.Next;
                            }

                            //add local angled instructions to cache
                            foreach (var ins in localInsts)
                                if (!compilerInstructions.Contains(ins))
                                    compilerInstructions.Add(ins);
                            return true;
                        }
                    }
                    instr = instr.Previous;
                }
                return false;
            }

            bool IsNextReturn(int ind)
            {
                var ins = instructions[ind];
                var op = ins.Operand as Instruction;
                if (op == null)
                    return false;
                if (op == lastOp && ins.Next == lastOp)
                    return true;
                return op.OpCode.Name.StartsWith("ldloc") && (op.Next == lastOp || op.Next?.Next == lastOp) ? true : false;
            }
            #endregion
        }

        internal string GetDestinationDirectory(MainOptions opts, string sourceDir)
        {
            string destDir;
            if (IoHelper.IsSameDirectories(sourceDir, opts.Source.Directory))
            {
                destDir = opts.Destination.Directory;
            }
            else
            {
                destDir = Path.Combine(opts.Destination.Directory, sourceDir.Remove(0, opts.Source.Directory.Length));
            }
            return destDir;
        }

        internal MethodType GetMethodType(MethodDefinition def)
        {
            string methodFullName = def.FullName;
            var type = def.DeclaringType;
            var declAttrs = type.CustomAttributes;
            var compGenAttrName = typeof(CompilerGeneratedAttribute).Name;
            var isCompilerGeneratedType = declAttrs
                .FirstOrDefault(a => a.AttributeType.Name == compGenAttrName) != null; //methodName.StartsWith("<"); 
            //                                                                                                          
            if (def.IsSetter)
                return MethodType.Setter;
            if (def.IsGetter)
                return MethodType.Getter;
            if (methodFullName.Contains("::add_"))
                return MethodType.EventAdd;
            if (methodFullName.Contains("::remove_"))
                return MethodType.EventRemove;
            //if (methodFullName.StartsWith("<>f__"))
            //    return MethodType.Anonymous;
            //
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

        internal void CopySource([NotNull] string sourcePath, [NotNull] string destPath)
        {
            if (Directory.Exists(destPath))
                Directory.Delete(destPath, true);
            Directory.CreateDirectory(destPath);
            IoHelper.DirectoryCopy(sourcePath, destPath);
        }

        internal AssemblyVersion GetAssemblyVersion([NotNull] string filePath)
        {
            var asmName = AssemblyName.GetAssemblyName(filePath);
            if (asmName.ProcessorArchitecture != ProcessorArchitecture.MSIL)
                return new AssemblyVersion() { Target = AssemblyVersionType.NotIL };
            if (!asmName.FullName.EndsWith("PublicKeyToken=null"))
            {
                //log: is strong name!
                return new AssemblyVersion() { IsStrongName = true };
            }
            var asm = Assembly.LoadFrom(filePath);
            var versionAttr = asm.CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(System.Runtime.Versioning.TargetFrameworkAttribute));
            var versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
            var version = new AssemblyVersion(versionS);
            return version;
        }

        internal List<MethodDefinition> GetAllMethods(TypeDefinition type, [NotNull] MainOptions opts)
        {
            var methods = new List<MethodDefinition>();

            //own
            var probOpts = opts.Probes;
            var isAngleBracket = type.Name.StartsWith("<");
            var nestedMeths = type.Methods
                .Where(a => a.HasBody)
                .Where(a => !(isAngleBracket && a.IsConstructor)) //internal compiler's ctor is not needed in any cases
                .Where(a => probOpts.Ctor || (!probOpts.Ctor && !a.IsConstructor)) //may be we skips own ctors
                .Where(a => probOpts.Setter || (!probOpts.Setter && a.Name != "set_Prop")) //do we need property setters?
                .Where(a => probOpts.Getter || (!probOpts.Getter && a.Name != "get_Prop")) //do we need property getters?
                .Where(a => type.Name.StartsWith("<") || probOpts.Private || (!probOpts.Private && !a.IsPrivate)) //do we need business privates?
                ;
            foreach (var nestedMethod in nestedMeths)
                methods.Add(nestedMethod);

            //nested
            foreach (var nestedType in type.NestedTypes)
            {
                var innerMethods = GetAllMethods(nestedType, opts);
                methods.AddRange(innerMethods);
            }
            return methods;
        }

        internal string GetProbeData(InjectedMethod injMeth, MethodDefinition def, string moduleName, 
                                     string reaMethodName, string fullMethodName, CrossPointType pointType, 
                                     int localId)
        {
            var id = localId == -1 ? null : localId.ToString();
            Interlocked.Add(ref _curPointUid, 1);

            var crossPoint = new CrossPoint(_curPointUid.ToString(), id, pointType)
            {
                //PDB data
            };
            injMeth.AddChild(crossPoint);

            return $"{reaMethodName}^{moduleName}^{fullMethodName}^{_curPointUid}";
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

        private MethodSource CreateMethodSource(MethodDefinition def, MethodType methodType, bool isNested, string hashCode)
        {
            return new MethodSource
            {
                AccessType = GetAccessType(def),
                IsAbstract = def.IsAbstract,
                IsGeneric = def.HasGenericParameters,
                IsStatic = def.IsStatic,
                MethodType = methodType,
                //IsOverride = ...
                IsNested = isNested,
                HashCode = hashCode,
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
            var path = GetTreeFilePath(tree);
            _rep.WriteInjectedTree(path, tree);
        }

        internal async void NotifyAboutTree(InjectedSolution tree)
        {
            //in each folder create file with path to tree data
            var dirs = tree.GetAllDirectories();
            var pathInText = GetTreeFilePath(tree);
            foreach (var dir in dirs)
            {
                var hintPath = GetTreeFileHintPath(dir.DestinationPath);
                await File.WriteAllTextAsync(hintPath, pathInText);
            }
        }

        internal string GetTreeFilePath(InjectedSolution tree)
        {
            return Path.Combine(tree.DestinationPath, CoreConstants.TREE_FILE_NAME);
        }
        
        internal string GetTreeFileHintPath(string path)
        {
            return Path.Combine(path, CoreConstants.TREE_FILE_HINT_NAME);
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Injector.Core
{
    public class InjectorEngine
    {
        /* INFO *
            http://ilgenerator.apphb.com/ - online C# -> IL
            https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C# -
                on Github - https://github.com/adrianoc/cecilifier
            https://www.codeproject.com/Articles/671259/Reweaving-IL-code-with-Mono-Cecil
            https://blog.elishalom.com/2012/02/04/monitoring-execution-using-mono-cecil/
            https://stackoverflow.com/questions/48090703/run-mono-cecil-in-net-core
        */

        private readonly IInjectorRepository _rep;
        private readonly ThreadLocal<bool?> _isNetCore;
        private readonly ThreadLocal<AssemblyVersion> _mainVersion;
        private readonly EnumerationOptions _searchOpts;

        /***************************************************************************************/

        public InjectorEngine()
        {
            _rep = new InjectorRepository();
            _isNetCore = new ThreadLocal<bool?>();
            _mainVersion = new ThreadLocal<AssemblyVersion>();
            _searchOpts = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false,
                RecurseSubdirectories = false
            };
        }

        /***************************************************************************************/

        public void Process([NotNull] string[] args)
        {
            var opts = _rep.CreateOptions(args);
            Process(opts);
        }

        public void Process([NotNull] InjectOptions opts)
        {
            _rep.ValidateOptions(opts);
            CopySource(opts.SourceDirectory, opts.DestinationDirectory);
            var versions = DefineTargetVersions(opts.SourceDirectory);
            ProcessDirectory(opts.SourceDirectory, versions, opts);
        }

        internal void ProcessDirectory([NotNull] string directory, [NotNull] Dictionary<string, AssemblyVersion> versions, [NotNull] InjectOptions opts)
        {
            //files
            var files = IoHelper.GetAssemblies(directory, _searchOpts);
            foreach (var file in files)
            {
                ProcessAssembly(file, versions, opts);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*", _searchOpts);
            foreach (var dir in dirs)
            {
                ProcessDirectory(dir, versions, opts);
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

        public void ProcessAssembly([NotNull] string filePath, [NotNull] Dictionary<string, AssemblyVersion> versions, [NotNull] InjectOptions opts)
        {
            #region Reading
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");

            var sourceDir = $"{Path.GetFullPath(Path.GetDirectoryName(filePath))}\\";
            Environment.CurrentDirectory = sourceDir;
            var subjectName = Path.GetFileNameWithoutExtension(filePath);

            #region Destinaton
            string destDir;
            if (IoHelper.IsSameDirectories(sourceDir, opts.SourceDirectory))
            {
                destDir = opts.DestinationDirectory;
            }
            else
            {
                destDir = Path.Combine(opts.DestinationDirectory, sourceDir.Remove(0, opts.SourceDirectory.Length));
            }
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            #endregion

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
            AssemblyDefinition assembly = null;
            try
            {
                assembly = AssemblyDefinition.ReadAssembly(filePath, readerParams);
            }
            catch
            {
                //log
            }
            var module = assembly.MainModule;
            #endregion
            #region Commands

            // 1. 'WriteLine' command
            MethodReference consoleWriteLine;
            //if (_isNetCore.Value == true)
            //{
            //    // get path to 'System.Console' reference assembly and read it
            //    var consoleFileName = SdkHelper.GetCoreAssemblyPath(_mainVersion.Value.Version, "System.Console");
            //    var consoleDefinition = AssemblyDefinition.ReadAssembly(consoleFileName);

            //    // get 'System.Console' type and 'Console.WriteLine(string)' method and import it
            //    consoleWriteLine = consoleDefinition.MainModule.GetType(typeof(Console).FullName)
            //        .Methods
            //        .FirstOrDefault(x =>
            //            string.Equals(x.Name, nameof(Console.WriteLine), StringComparison.InvariantCultureIgnoreCase) &&
            //            x.Parameters.Count == 1 &&
            //            string.Equals(x.Parameters[0].ParameterType.FullName, typeof(string).FullName, StringComparison.InvariantCultureIgnoreCase)
            //        );
            //    var consoleWriteLineRef = module.ImportReference(consoleWriteLine);
            //}
            //else
            //{
            //TODO: choose the correct version of Net Framework!

            //https://jeremybytes.blogspot.com/2020/01/using-typegettype-with-net-core.html
            //var t = typeof(Console);
            //AssemblyLoadContext.Default.LoadFromAssemblyPath(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.3\System.Console.dll");
            //AssemblyLoadContext.Default.LoadFromAssemblyPath(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll");
            //var t = Type.GetType("System.Console, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"); //load current, not specified

            ////TODO: for misc frameworks including Net Framework
            //var path = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Console.dll";
            //var loadContext = new AssemblyContext(path);
            //var asm = loadContext.ResolveAssemblyToPath(path); //asm successful loading
            //var consoleType = asm.CreateInstance("System.Console"); //fail on loading mscorlib.dll
            //var consoleType = asm.ExportedTypes.FirstOrDefault(a => a.FullName == "System.Console"); //no loaded types

            var path = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Plugins.Logger\bin\Debug\netstandard2.0\Plugins.Logger.dll";
            var loadContext = new AssemblyContext(path);
            var injAsm = loadContext.ResolveAssemblyToPath(path); 
            var consoleType = injAsm.ExportedTypes.FirstOrDefault(a => a.FullName == "Plugins.Logger.LoggerPlugin");
            var method = consoleType.GetMethod("Process", new Type[] { typeof(string) });
            consoleWriteLine = module.ImportReference(method);

            //var injAsmName = injAsm.GetName();
            //var injAsmRef = new AssemblyNameReference(injAsmName.Name, injAsmName.Version);
            //module.AssemblyReferences.Add(injAsmRef);

            //consoleWriteLine = module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }));
            //}

            // 2. 'Call' command
            var call = Instruction.Create(OpCodes.Call, consoleWriteLine);
            #endregion
            #region Processing
            HashSet<Instruction> jumpers;
            Mono.Collections.Generic.Collection<Instruction> instructions;
            HashSet<Instruction> compilerInstructions;
            var compGenAttrName = typeof(CompilerGeneratedAttribute).Name;
            var dbgHiddenAttrName = typeof(DebuggerHiddenAttribute).Name;
            bool isAsyncStateMachine = false;

            foreach (TypeDefinition type in module.Types)
            {
                if (type.Name == "<Module>")
                    continue;

                //collect methods including business & compiler's nested classes (for async, delegates, anonymous types...)
                var methods = GetAllMethods(type, opts);

                //process all methods
                foreach (var methodDefinition in methods)
                {
                    #region Init
                    var realType = methodDefinition.DeclaringType;
                    var typeName = realType.FullName;
                    var methodName = methodDefinition.Name;
                    var isFinalizer = methodName == "Finalize" && methodDefinition.IsVirtual;

                    //instructions
                    var body = methodDefinition.Body;
                    instructions = body.Instructions; //no copy list!
                    var processor = body.GetILProcessor();
                    compilerInstructions = new HashSet<Instruction>();
                    var startInd = 1;

                    //method's attributes
                    var methAttrs = methodDefinition.CustomAttributes;
                    var isDbgHidden = methAttrs.FirstOrDefault(a => a.AttributeType.Name == dbgHiddenAttrName) != null;
                    if (isDbgHidden)
                        continue;

                    //check for async/await
                    var interfaces = realType.Interfaces;
                    isAsyncStateMachine = interfaces.FirstOrDefault(a => a.InterfaceType.Name == "IAsyncStateMachine") != null;
                    if (isAsyncStateMachine) //skip state machine init jump block
                        startInd = 12;

                    //type's attributes
                    var declAttrs = realType.CustomAttributes;
                    var needEnterLeavings =
                        //Async/await
                        !isAsyncStateMachine && declAttrs.FirstOrDefault(a => a.AttributeType.Name == compGenAttrName) == null && //!methodName.StartsWith("<");
                        //Finalyze() -> strange, but for Core 'Enter' & 'Leaving' lead to a crash here                   
                        (_isNetCore.Value == false || (_isNetCore.Value == true && !isFinalizer));
                    #endregion
                    #region Enter/Leaving
                    //inject 'entering' instruction
                    Instruction ldstrEntering = null;
                    if (needEnterLeavings)
                    {
                        var firstOp = instructions.First();
                        ldstrEntering = Instruction.Create(OpCodes.Ldstr, $"\n>> {typeName}.{methodName}");
                        processor.InsertBefore(firstOp, ldstrEntering);
                        processor.InsertBefore(firstOp, call);
                    }

                    //'leaving' instruction without immediate injection
                    var ldstrLeaving = Instruction.Create(OpCodes.Ldstr, $"<< {typeName}.{methodName}");
                    var lastOp = instructions.Last();
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
                    #region Misc injections               
                    var ifStack = new Stack<Instruction>();
                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var opCode = instr.OpCode;
                        var code = opCode.Code;
                        var flow = opCode.FlowControl;

                        if (instr.Operand == lastOp && needEnterLeavings) //jump to the end for return from function
                            instr.Operand = ldstrLeaving;

                        // IF/SWITCH
                        if (flow == FlowControl.Cond_Branch)
                        {
                            if (!isAsyncStateMachine && IsCompilerBranch(i))
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
                                var ind = instructions.IndexOf(operand); //inefficient, but it will be rarely...
                                var prevOperand = SkipNop(ind, false);
                                if (prevOperand.OpCode.Code == Code.Br || prevOperand.OpCode.Code == Code.Br_S) //while
                                {
                                    var ldstrIf2 = GetForIfInstruction();
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

                            // if/switch
                            ifStack.Push(instr);
                            if (code == Code.Switch)
                            {
                                for (var k = 0; k < ((Instruction[])instr.Operand).Length - 1; k++)
                                    ifStack.Push(instr);
                            }

                            var ldstrIf = isBrFalse ? GetForIfInstruction() : GetForElseInstruction();

                            //when inserting 'after', must set in desc order
                            processor.InsertAfter(instr, call);
                            processor.InsertAfter(instr, ldstrIf);

                            //correct jump instruction
                            if (operand != null)
                            {
                                var newOpCode = ConvertShortJumpToLong(opCode);
                                if (newOpCode.Code != opCode.Code)
                                {
                                    //EACH short form -> to long form (otherwise, you need to recalculate 
                                    //again after each necessary conversion)

                                    //var injectLen = ldstrIf.GetSize() + call.GetSize();
                                    //var diff = operand.Offset - instr.Offset;
                                    //if (diff + injectLen > 127 || diff < -128) //is too far?
                                    {
                                        var longIstr = Instruction.Create(newOpCode, operand);
                                        processor.Replace(instr, longIstr);
                                        jumpers.Remove(instr);
                                        jumpers.Add(longIstr);
                                    }
                                }
                            }
                            i += 2;
                            continue;
                        }

                        // ELSE/JUMP
                        if (flow == FlowControl.Branch && (code == Code.Br || code == Code.Br_S))
                        {
                            if (!ifStack.Any())
                                continue;
                            if (!isAsyncStateMachine && IsCompilerBranch(i))
                                continue;
                            if (!IsRealCondition(i)) //is real forward condition's branch?
                                continue;
                            //
                            var ifInst = ifStack.Pop();
                            var pairedCode = ifInst.OpCode.Code;
                            var elseInst = pairedCode == Code.Brfalse || pairedCode == Code.Brfalse_S ? GetForElseInstruction() : GetForIfInstruction();
                            var instr2 = instructions[i + 1];
                            ReplaceJump(instr2, elseInst);

                            processor.InsertBefore(instr2, elseInst); 
                            processor.InsertBefore(instr2, call);
                            i += 2;
                            continue;
                        }

                        //THROW
                        if (flow == FlowControl.Throw)
                        {
                            var throwInst = GetForThrowInstruction();
                            ReplaceJump(instr, throwInst);
                            processor.InsertBefore(instr, throwInst);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }

                        //RETURN
                        // TODO: check FAULT-block from VisualBasic (it should work with same OpCode)
                        if (flow == FlowControl.Return && needEnterLeavings && code != Code.Endfinally) //&& code != Code.Endfilter ???
                        {
                            ReplaceJump(instr, ldstrLeaving);
                            processor.InsertBefore(instr, ldstrLeaving);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }
                    } //cycle
                    #endregion
                }
            }

            // ensure we referencing only ref assemblies
            var systemPrivateCoreLib = module.AssemblyReferences.FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
            //Debug.Assert(systemPrivateCoreLib == null, "systemPrivateCoreLib == null");
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
            #endregion

            Console.WriteLine($"Modified assembly is created: {modifiedPath}");

            #region Local functions

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
                //var offsetS = string.Empty;
                //if (int.TryParse((op.Operand as Instruction)?.Operand?.ToString(), out int offset)) //is a jump?
                //    offsetS = offset.ToString();
                return op.Operand != next /*&& offsetS != next.Offset.ToString()*/; //how far do it jump?
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

            bool IsCompilerBranch(int ind)
            {
                //TODO: optimize (caching 'normal instruction')!!!
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

                    //we don't need 'angled' instructions in business code
                    if (!compilerInstructions.Contains(instr))
                    {
                        localInsts.Add(instr);
                        var operand = instr.Operand as MemberReference;
                        if (operand?.Name.StartsWith("<") == true)
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
            #endregion
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

        private List<MethodDefinition> GetAllMethods(TypeDefinition type, [NotNull] InjectOptions opts)
        {
            var methods = new List<MethodDefinition>();

            //own
            var isAngleBracket = type.Name.StartsWith("<");
            var nestedMeths = type.Methods
                .Where(a => a.HasBody)
                .Where(a => !(isAngleBracket && a.IsConstructor)) //internal compiler's ctor is not needed in any cases
                .Where(a => opts.InjectConstructors || (!opts.InjectConstructors && !a.IsConstructor)) //may be we skips own ctors
                .Where(a => opts.InjectSetters || (!opts.InjectSetters && a.Name != "set_Prop")) //do we need property setters?
                .Where(a => opts.InjectGetters || (!opts.InjectGetters && a.Name != "get_Prop")) //do we need property getters?
                .Where(a => opts.InjectPrivates || (!opts.InjectPrivates && !a.IsPrivate)) //do we need property privates?
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

        private Instruction GetForIfInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"-> IF");
        }

        private Instruction GetForElseInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"-> ELSE");
        }

        private Instruction GetForThrowInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"<< THROW");
        }

        //http://www.swat4net.com/mono-cecil-part-ii-basic-operations/
        //public static void ReweaveMethods(List<MethodDefinition> targetMethods)
        //{
        //    // we need to rescue the string.concat method which lives in mscorlib.dll
        //    var assemblyMSCorlib = AssemblyDefinition.ReadAssembly(Console.Default.ILMsCorlibPath);

        //    foreach (var method in targetMethods)
        //    {
        //        try
        //        {
        //            var methodToInject = assemblyMSCorlib.MainModule.Types.Where(o => o.IsClass == true)
        //                                                                  .SelectMany(type => type.Methods)
        //                                                                  .Where(o => o.FullName.Contains("Concat(System.String,System.String)")).FirstOrDefault();

        //            var InstructionPattern = new Func<Instruction, bool>(x => (x.OpCode == OpCodes.Ldstr) && (x.Operand.ToString().Contains("Name")));
        //            var InstructionPatternSet = new Func<Instruction, bool>(x => (x.OpCode == OpCodes.Callvirt) && (x.Operand.ToString().Contains("ClientLibrary.Entities.Person::set_Name(System.String)")));

        //            if (null != methodToInject)
        //            {
        //                // importing aspect method 
        //                var methodReferenced = method.DeclaringType.Module.ImportReference(methodToInject);
        //                method.DeclaringType.Module.ImportReference(methodToInject.DeclaringType);

        //                // defining ILProcesor
        //                var processor = method.Body.GetILProcessor();

        //                method.Body.SimplifyMacros();

        //                // first update
        //                var writeLines = processor.Body.Instructions.Where(InstructionPattern).ToArray();
        //                foreach (var instruction in writeLines)
        //                {
        //                    var newInstruction = processor.Create(OpCodes.Ldstr, "Code Injected !");

        //                    Instruction targetReferenceInstruction = processor.Create(OpCodes.Nop);
        //                    targetReferenceInstruction = instruction.Previous;
        //                    processor.InsertBefore(targetReferenceInstruction, newInstruction);
        //                }

        //                // second update
        //                var writeLinesSet = processor.Body.Instructions.Where(InstructionPatternSet).ToArray();
        //                foreach (var instruction in writeLinesSet)
        //                {
        //                    var concatInstruction = processor.Create(OpCodes.Call, methodReferenced);
        //                    processor.InsertBefore(instruction, concatInstruction);
        //                }

        //                method.Body.OptimizeMacros();

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            //throw;
        //        }
        //    }
        //}
    }
}
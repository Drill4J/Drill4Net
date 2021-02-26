using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Mono.Cecil.Rocks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace TestA.Interceptor
{
    /* INFO *
        https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C#
    */

    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length != 1)
            //{
            //    Console.WriteLine("TraceIL.exe <assembly>");
            //    return;
            //}

            //string filename = args[0];

            var filename = @"d:\Projects\EPM-D4J\!!_exp\TestA\TestNF\bin\Debug\TestNF.exe";
            var isSetGetIncluding = false;
            //TODO: checking the constructor traversal?

            ModuleDefinition module = ModuleDefinition.ReadModule(filename);
            MethodReference consoleWriteLine =
                module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }));
            var call = Instruction.Create(OpCodes.Call, consoleWriteLine);
            List<Instruction> jumpers;
            Collection<Instruction> instructions;
            HashSet<Instruction> compilerInstructions;
            var compGenAttrName = typeof(CompilerGeneratedAttribute).Name;
            var dbgHiddenAttrName = typeof(DebuggerHiddenAttribute).Name;
            bool isAsyncStateMachine = false;

            foreach (TypeDefinition type in module.Types)
            {
                //collect methods including business & compiler's nested classes (for async, delegates, anonymous types...)
                var methods = GetAllMethods(type, isSetGetIncluding);

                //process all methods
                foreach (var methodDefinition in methods)
                {
                    var realType = methodDefinition.DeclaringType;
                    var typeName = realType.FullName;
                    var methodName = methodDefinition.Name;

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
                    if (isAsyncStateMachine) //seems not good: skip state machine's initial instructions
                        startInd = 12;

                    //type's attributes
                    var declAttrs = realType.CustomAttributes;
                    var needEnterLeavings = !isAsyncStateMachine && declAttrs.FirstOrDefault(a => a.AttributeType.Name == compGenAttrName) == null; //!methodName.StartsWith("<");

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

                    //collect jumps. Hash table for addresses is almost useless,
                    //because they will be recalculated inside the processor during inject...
                    //and ideally, there shouldn't be too many of them 
                    jumpers = new List<Instruction>();
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var flow = instr.OpCode.FlowControl;
                        if (flow == FlowControl.Branch || flow == FlowControl.Cond_Branch)
                            jumpers.Add(instr);
                    }

                    //misc injections               
                    var ifStack = new Stack<Instruction>();
                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var code = instr.OpCode.Code;
                        var flow = instr.OpCode.FlowControl;

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
                            var isBrFalse = code == Code.Brfalse || code == Code.Brfalse_S;

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

                            //when inserting 'after', let set in desc order
                            processor.InsertAfter(instr, call);
                            processor.InsertAfter(instr, ldstrIf);
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
                            var op2 = instructions[i + 1]; //TODO: check for overflow!

                            //ifInst.Operand = elseInst; //re-address 'else'
                            CorrrectJump(op2, elseInst);
                            processor.InsertBefore(op2, elseInst);
                            processor.InsertBefore(op2, call);
                            i += 2;

                            continue;
                        }

                        //THROW
                        if (flow == FlowControl.Throw)
                        {
                            var throwInst = GetForThrowInstruction();
                            processor.InsertBefore(instr, throwInst);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }

                        //RETURN
                        if (flow == FlowControl.Return && needEnterLeavings && code != Code.Endfinally) // && code != Code.Endfilter ???
                        {
                            CorrrectJump(instr, ldstrEntering);
                            processor.InsertBefore(instr, ldstrLeaving);
                            processor.InsertBefore(instr, call);
                            i += 2;
                            continue;
                        }
                    } //cycle
                }
            }

            var modifiedPath = Path.GetFileNameWithoutExtension(filename) + ".modified" + Path.GetExtension(filename);
            module.Write(modifiedPath);

            Console.WriteLine($"Modified assembly is created: {modifiedPath}");
            Console.ReadKey(true);

            // local functions //

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
                    var isInternal = prev.OpCode.Code == Code.Call && prevOpS != null && (prevOpS.EndsWith("TaskAwaiter::get_IsCompleted()") || prevOpS.Contains("TaskAwaiter`1"));
                    if (isInternal)
                        return false;
                }
                //
                var next = SkipNop(ind, true);
                var offsetS = string.Empty;
                if (int.TryParse((op.Operand as Instruction)?.Operand?.ToString(), out int offset)) //is a jump?
                    offsetS = offset.ToString();
                return op.Operand != next && offsetS != next.Offset.ToString(); //how far do it jump?
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

            void CorrrectJump(Instruction from, Instruction to)
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
        }

        private static List<MethodDefinition> GetAllMethods(TypeDefinition type, bool isSetGetIncluding)
        {
            var methods = new List<MethodDefinition>();

            //own
            var isAngleBracket = type.Name.StartsWith("<");
            var nestedMeths = type.Methods
                .Where(a => a.HasBody)
                .Where(a => !(isAngleBracket && a.IsConstructor))
                .Where(a => isSetGetIncluding || (!isSetGetIncluding && a.Name != "get_Prop" && a.Name != "set_Prop"))
                ;
            foreach (var nestedMethod in nestedMeths)
                methods.Add(nestedMethod);

            //nested
            foreach (var nestedType in type.NestedTypes)
            {
                var innerMethods = GetAllMethods(nestedType, isSetGetIncluding);
                methods.AddRange(innerMethods);
            }
            return methods;
        }

        private static Instruction GetForIfInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"-> IF");
        }

        private static Instruction GetForElseInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"-> ELSE");
        }

        private static Instruction GetForThrowInstruction()
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

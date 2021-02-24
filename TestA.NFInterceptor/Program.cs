using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Mono.Cecil.Rocks;

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

            ModuleDefinition module = ModuleDefinition.ReadModule(filename);
            MethodReference consoleWriteLine =
                module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }));
            var call = Instruction.Create(OpCodes.Call, consoleWriteLine);
            List<Instruction> jumpers;
            Collection<Instruction> instructions;

            foreach (TypeDefinition type in module.Types)
            {
                //collect methods including for all nested classes (delegates, anonymous types...)
                var methods = type.Methods.Where(a => a.HasBody).ToList(); //need copy list
                foreach (var nestedType in type.NestedTypes)
                {
                    var isAngleBracket = nestedType.Name.StartsWith("<");
                    foreach (var nestedMethod in nestedType.Methods.Where(a => a.HasBody && !(isAngleBracket && a.IsConstructor)))
                        methods.Add(nestedMethod);
                }

                //process all methods
                foreach (var methodDefinition in methods) 
                {
                    var methodName = methodDefinition.Name;
                    var needEnterLeavings = !methodName.StartsWith("<");
                    var body = methodDefinition.Body;
                    if (body == null)
                        continue;
                    var processor = body.GetILProcessor();
                    instructions = body.Instructions;//no copy list!

                    //inject 'entering' instruction
                    Instruction ldstrEntering = null;
                    if (needEnterLeavings)
                    {
                        var firstOp = instructions.First();
                        ldstrEntering = Instruction.Create(OpCodes.Ldstr, $"\n>> {methodName}");
                        processor.InsertBefore(firstOp, ldstrEntering);
                        processor.InsertBefore(firstOp, call);
                    }

                    //'leaving' instruction without immediate injection
                    var ldstrLeaving = Instruction.Create(OpCodes.Ldstr, $"<< {methodName}");
                    var lastOp = instructions.Last();

                    //collect jumps. Hash table for addresses is almost useless,
                    //because they will be recalculated inside the processor during inject...
                    //and ideally, there shouldn't be too many of them 
                    jumpers = new List<Instruction>();
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var op = instructions[i];
                        var flow = op.OpCode.FlowControl;
                        if (flow == FlowControl.Branch || flow == FlowControl.Cond_Branch)
                            jumpers.Add(op);
                    }

                    //misc injections               
                    var ifStack = new Stack<Instruction>();
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var op = instructions[i];
                        var code = op.OpCode.Code;
                        var flow = op.OpCode.FlowControl;

                        if (op.Operand == lastOp && needEnterLeavings) //jump to the end for return from function
                            op.Operand = ldstrLeaving;

                        // IF/SWITCH
                        if (flow == FlowControl.Cond_Branch)
                        {
                            if (op.Operand is Instruction operand && operand.Offset > 0 && op.Offset > operand.Offset) //operators: while, do
                            {
                                var ind = instructions.IndexOf(operand); //inefficient, but it will be rarely...
                                var prevOperand = SkipNop(ind, false);
                                if (prevOperand.OpCode.Code == Code.Br || prevOperand.OpCode.Code == Code.Br_S) //while
                                {
                                    var ldstrIf = GetForIfInstruction();
                                    var targetOp = prevOperand.Operand as Instruction;
                                    processor.InsertBefore(targetOp, ldstrIf);
                                    processor.InsertBefore(targetOp, call);
                                    i += 2;
                                }
                                else //do
                                { 
                                    //no signaling...
                                }
                                continue;
                            }
                            //
                            if (op.Operand != SkipNop(i, true)) //is real forward condition's branch?
                            {
                                ifStack.Push(op);
                                if (code == Code.Switch)
                                {
                                    for (var k = 0; k < ((Instruction[])op.Operand).Length - 1; k++)
                                        ifStack.Push(op);
                                }

                                var ldstrIf = code == Code.Brfalse || code == Code.Brfalse_S ? GetForIfInstruction() : GetForElseInstruction();

                                //when inserting 'after', let set in desc order
                                processor.InsertAfter(op, call);
                                processor.InsertAfter(op, ldstrIf);
                                i += 2;
                            }
                        }

                        // ELSE/JUMP
                        if (flow == FlowControl.Branch && (code == Code.Br || code == Code.Br_S))
                        {
                            if (ifStack.Any() && op.Operand != SkipNop(i, true)) //is real forward condition's branch?
                            {
                                var ifInst = ifStack.Pop();
                                var pairedCode = ifInst.OpCode.Code;
                                var elseInst = pairedCode == Code.Brfalse || pairedCode == Code.Brfalse_S ? GetForElseInstruction() : GetForIfInstruction();
                                var op2 = instructions[i + 1]; //TODO: check for overflow!

                                //ifInst.Operand = elseInst; //re-address 'else'
                                CorrrectJump(op2, elseInst);
                                processor.InsertBefore(op2, elseInst);
                                processor.InsertBefore(op2, call);
                                i += 2;
                            }
                        }

                        //THROW
                        if (flow == FlowControl.Throw)
                        {
                            var throwInst = GetForThrowInstruction();
                            processor.InsertBefore(op, throwInst);
                            processor.InsertBefore(op, call);
                            i += 2;
                        }

                        //RETURN
                        if (flow == FlowControl.Return && needEnterLeavings)
                        {
                            CorrrectJump(op, ldstrEntering);
                            processor.InsertBefore(op, ldstrLeaving);
                            processor.InsertBefore(op, call);
                            i += 2;
                        }
                    }
                }
            }

            var modifiedPath = Path.GetFileNameWithoutExtension(filename) + ".modified" + Path.GetExtension(filename);
            module.Write(modifiedPath);

            Console.WriteLine($"Modified assembly is created: {modifiedPath}");
            Console.ReadKey(true);

            // local functions //

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

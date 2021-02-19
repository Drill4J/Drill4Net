﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
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

            foreach (TypeDefinition type in module.Types)
            {
                foreach (var methodDefinition in type.Methods)
                {
                    var methodName = methodDefinition.Name;
                    var body = methodDefinition.Body;
                    var processor = body.GetILProcessor();
                    var instructions = body.Instructions;

                    //inject first instruction
                    var firstOp = instructions.First();
                    var ldstrEntering = Instruction.Create(OpCodes.Ldstr, $"\n>> {methodName}");
                    processor.InsertBefore(firstOp, ldstrEntering);
                    processor.InsertBefore(firstOp, call);

                    //inject last instruction
                    var ldstrLeaving = Instruction.Create(OpCodes.Ldstr, $"<< {methodName}");
                    var lastOp = instructions.Last();
                    processor.InsertBefore(lastOp, ldstrLeaving);
                    processor.InsertBefore(lastOp, call);

                    //misc injections
                    //var ldstrBranch = Instruction.Create(OpCodes.Ldstr, $"-> BRANCH");                 
                    var ifStack = new Stack<Instruction>();
                    for (var i = 1; i < instructions.Count - 1; i++)
                    {
                        var op = instructions[i];
                        var code = op.OpCode.Code;
                        var flow = op.OpCode.FlowControl;

                        //IF
                        if (flow == FlowControl.Cond_Branch)
                        {
                            ifStack.Push(op);                
                            var ldstrIf = code == Code.Brfalse || code == Code.Brfalse_S ? GetForIfInstruction() : GetForElseInstruction();

                            //when "after" set in desc order
                            processor.InsertAfter(op, call);
                            processor.InsertAfter(op, ldstrIf);
                            i += 2;
                        }

                        //ELSE
                        if (flow == FlowControl.Branch && (code == Code.Br || code == Code.Br_S))
                        {
                            var ifInst = ifStack.Pop();
                            var pairedCode = ifInst.OpCode.Code;
                            var elseInst = pairedCode == Code.Brfalse || pairedCode == Code.Brfalse_S ? GetForElseInstruction() : GetForIfInstruction();

                            ifInst.Operand = elseInst; //readdress 'else'
                            var op2 = instructions[i + 1];
                            processor.InsertBefore(op2, elseInst);
                            processor.InsertBefore(op2, call);

                            i += 2;
                        }

                        //inject after return & throw
                        if (/*op.OpCode.Code == Code.Leave_S || */flow == FlowControl.Return || flow == FlowControl.Throw)
                        {
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
        }

        private static Instruction GetForIfInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"-> IF");
        }

        private static Instruction GetForElseInstruction()
        {
            return Instruction.Create(OpCodes.Ldstr, $"-> ELSE");
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

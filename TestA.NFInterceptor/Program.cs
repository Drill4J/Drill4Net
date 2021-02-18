using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TestA.Interceptor
{
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

            foreach (TypeDefinition type in module.Types)
            {
                foreach (var methodDefinition in type.Methods)
                {
                    var ilBody = methodDefinition.Body;
                    var ilProcessor = ilBody.GetILProcessor();

                    var firstOp = methodDefinition.Body.Instructions.First();
                    var ldstrEntering = Instruction.Create(OpCodes.Ldstr, $"--Entering {methodDefinition.Name}");
                    ilProcessor.InsertBefore(firstOp, ldstrEntering);
                    var call = Instruction.Create(OpCodes.Call, consoleWriteLine);
                    ilProcessor.InsertBefore(firstOp, call);
                    var ldstrLeaving = Instruction.Create(OpCodes.Ldstr, $"--Leaving {methodDefinition.Name}");
                    var lastOp = methodDefinition.Body.Instructions.Last();
                    ilProcessor.InsertBefore(lastOp, ldstrLeaving);
                    ilProcessor.InsertBefore(lastOp, call);
                }
            }

            var modifiedPath = Path.GetFileNameWithoutExtension(filename) + ".modified" + Path.GetExtension(filename);
            module.Write(modifiedPath);

            Console.WriteLine($"Modified assembly is created: {modifiedPath}");
            Console.ReadKey(true);
        }
    }
}

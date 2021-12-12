using System;
using Drill4Net.Common;

namespace Drill4Net.AssemblyInstruction.Helper.Debug
{
    class Program
    {
        const string ASSEMBLY_PATH = @"..\..\Drill4Net.AsyncTest\net5.0\Drill4Net.AsyncTest.dll";
        static void Main(string[] args)
        {
            var assembly = FileUtils.GetFullPath(ASSEMBLY_PATH);
            var instructionManager= new AsyncInstructionManager(assembly);
            foreach(var method in instructionManager.AsyncMethodInfo)
            {
                Console.WriteLine($"**********\n{method.MethodName}\n**********");
                method.PrintUserInstructions();
            }
        }
    }
}

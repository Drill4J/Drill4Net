using System;

namespace Drill4Net.AssemblyInstruction.Helper.Debug
{
    class Program
    {
        const string ASSEMBLY_FULL_NAME = @"C:\_Repos\Drill4Net\build\bin\Debug\Drill4Net.AsyncTest\net5.0\Drill4Net.AsyncTest.dll";
        static void Main(string[] args)
        {
            
            var instructionManager= new AsyncInstructionManager(ASSEMBLY_FULL_NAME);
        }
    }
}

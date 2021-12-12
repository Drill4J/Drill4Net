using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Drill4Net.AssemblyInstruction.Helper
{
    /// <summary>
    ///Represents method information.
    /// </summary>
    public class MethodInfo
    {
        public string MethodName {get; set;}
        public List<InstructionData> Instructions { get; set; }

        //**************************************************************************************//

        public MethodInfo(string methodName)
        {
            MethodName = methodName;
            Instructions = new List<InstructionData>();
        }

        public MethodInfo(string methodName, List<InstructionData> instructions)
        {
            Instructions = instructions;
            MethodName = methodName;
        }

        //**************************************************************************************//

        public List<Instruction> GetUserInstructions ()
        {
            return this.Instructions.Where(i => i.IsUserInst == true).Select(e => e.Inst).ToList();
        }

        public void PrintUserInstructions()
        {
            var instructions = GetUserInstructions();
            foreach(var inst in instructions)
            {
                Console.WriteLine(inst);
            }
        }

        public List<Instruction> GetCGInstructions()
        {
            return this.Instructions.Where(i => i.IsUserInst == false).Select(e => e.Inst).ToList();
        }
    }
}

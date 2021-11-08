using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.AssemblyInstruction.Helper
{
    public class MethodInfo
    {
        public string MethodName {get; set;}
        public List<InstructionData> Instructions { get; set; }
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
        public List<Instruction> GetUserInstruction ()
        {
            return this.Instructions.Where(i => i.IsUserInst == true).Select(e => e.Inst).ToList();
        }

        public List<Instruction> GetCGInstruction()
        {
            return this.Instructions.Where(i => i.IsUserInst == false).Select(e => e.Inst).ToList();
        }
    }
}

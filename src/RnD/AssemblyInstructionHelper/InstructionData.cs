using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.AssemblyInstruction.Helper
{ 
    public class InstructionData
    {
        public Instruction Inst { get; set; }
        public bool IsUserInst { get; set; }

        public InstructionData (Instruction inst, bool isUserInst)
        {
            Inst = inst;
            IsUserInst=isUserInst;
        }
    }
}

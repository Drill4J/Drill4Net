using Mono.Cecil.Cil;

namespace Drill4Net.AssemblyInstruction.Helper
{
    /// <summary>
    ///Represents instruction data.
    /// </summary>
    public class InstructionData
    {
        public Instruction Inst { get; set; }
        public bool IsUserInst { get; set; }

        /********************************************************************/

        public InstructionData (Instruction inst, bool isUserInst)
        {
            Inst = inst;
            IsUserInst=isUserInst;
        }
    }
}

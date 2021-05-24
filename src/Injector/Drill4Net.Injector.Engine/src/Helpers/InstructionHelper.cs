using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Instruction helper
    /// </summary>
    internal static class InstructionHelper
    {
        /// <summary>
        /// Converting the short form of an IL instruction to the long form in cases of "jumping too far"
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public static OpCode ShortJumpToLong(OpCode opCode)
        {
            //TODO: to a dictionary
            return opCode.Code switch
            {
                Code.Br_S => OpCodes.Br,
                Code.Brfalse_S => OpCodes.Brfalse,
                Code.Brtrue_S => OpCodes.Brtrue,
                Code.Beq_S => OpCodes.Beq,
                Code.Bge_S => OpCodes.Bge,
                Code.Bge_Un_S => OpCodes.Bge_Un,
                Code.Bgt_S => OpCodes.Bgt,
                Code.Bgt_Un_S => OpCodes.Bgt_Un,
                Code.Ble_S => OpCodes.Ble,
                Code.Ble_Un_S => OpCodes.Ble_Un,
                Code.Blt_S => OpCodes.Blt,
                Code.Blt_Un_S => OpCodes.Blt_Un,
                Code.Bne_Un_S => OpCodes.Bne_Un,
                Code.Leave_S => OpCodes.Leave,
                _ => opCode,
            };
        }
    }
}

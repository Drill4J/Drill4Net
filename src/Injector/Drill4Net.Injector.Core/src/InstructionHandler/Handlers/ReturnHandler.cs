﻿using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Core
{
    public class ReturnHandler : AbstractInstructionHandler
    {
        public ReturnHandler() : base(InjectorCoreConstants.INSTRUCTION_HANDLER_RETURN)
        {
        }

        /*****************************************************************************/

        protected override void HandleRequestConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var jumpers = ctx.Jumpers;

            var strictEnterReturn = ctx.IsStrictEnterReturn;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            var ldstrReturn = ctx.LdstrReturn;
            var returnProbData = ctx.ReturnProbData;
            #endregion

            //RETURN
            if (code == Code.Ret && !strictEnterReturn)
            {
                ldstrReturn.Operand = $"{returnProbData}{ctx.CurIndex}";
                FixFinallyEnd(instr, ldstrReturn, exceptionHandlers);
                ReplaceJump(instr, ldstrReturn, jumpers);
                processor.InsertBefore(instr, ldstrReturn);
                processor.InsertBefore(instr, call);
                ctx.IncrementIndex(2);
            }
        }
    }
}

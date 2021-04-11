﻿using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ThrowHandler : AbstractInstructionHandler
    {
        public ThrowHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_THROW, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var moduleName = ctx.ModuleName;
            var treeFunc = ctx.TreeMethod;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            //var code = opCode.Code;
            var flow = opCode.FlowControl;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var jumpers = ctx.Jumpers;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            //THROW
            if (flow != FlowControl.Throw) 
                return;

            //data
            var probData = GetProbeData(ctx);

            //injection
            var throwInst = GetFirstInstruction(probData);
            FixFinallyEnd(instr, throwInst, exceptionHandlers); //need fix statement boundaries for potential try/finally 
            ReplaceJump(instr, throwInst, jumpers);
            processor.InsertBefore(instr, throwInst);
            processor.InsertBefore(instr, call);
            ctx.IncrementIndex(2);
            
            needBreak = true;
        }

        protected virtual string GetProbeData(InjectorContext ctx)
        {
            return _probeHelper.GetProbeData(ctx, CrossPointType.Throw, ctx.CurIndex);
        }
    }
}

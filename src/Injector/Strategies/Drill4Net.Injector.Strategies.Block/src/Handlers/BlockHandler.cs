﻿using System;
using Drill4Net.Injector.Core;
using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockHandler : AbstractInstructionHandler
    {
        public BlockHandler(AbstractProbeHelper probeHelper) : 
            base(BlockConstants.INSTRUCTION_HANDLER_BLOCK, probeHelper)
        {
        }

        /************************************************************************************/

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var moduleName = ctx.ModuleName;
            var treeType = ctx.TreeType;
            var treeFunc = ctx.TreeMethod;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;
            var flow = opCode.FlowControl;

            var typeSource = treeType.SourceType;
            var methodSource = treeFunc.SourceType;

            var isAsyncStateMachine = typeSource.IsAsyncStateMachine;
            var isEnumeratorMoveNext = methodSource.IsEnumeratorMoveNext;

            var compilerInstructions = ctx.CompilerInstructions;
            var processed = ctx.Processed;
            var jumpers = ctx.Jumpers;

            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            if (flow == FlowControl.Next)
            {
            }

            if (   flow == FlowControl.Throw
                || flow == FlowControl.Return
                || flow == FlowControl.Branch
                || flow == FlowControl.Cond_Branch
                || flow == FlowControl.Break)
            {
                
            }
        }
    }
}

using System;
using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class NonConditionBranchHandler : AbstractInstructionHandler
    {
        public NonConditionBranchHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_NONCONDITIONAL, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var treeType = ctx.TreeType;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;
            var flow = opCode.FlowControl;
            var lastOp = ctx.LastOperation;

            var typeSource = treeType.SourceType;
            var isAsyncStateMachine = typeSource.IsAsyncStateMachine;
            var compilerInstructions = ctx.CompilerInstructions;
            var exceptionHandlers = ctx.ExceptionHandlers;
            var jumpers = ctx.Jumpers;
            var ifStack = ctx.IfStack;
            string probData;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion
            #region Checks
            if (flow != FlowControl.Branch || (code != Code.Br && code != Code.Br_S)) 
                return;
            if (!ifStack.Any())
                return;
            if (IsNextReturn(ctx.CurIndex, instructions, lastOp))
                return;
            if (!isAsyncStateMachine && IsCompilerGeneratedBranch(ctx.CurIndex, instructions, compilerInstructions))
                return;
            if (!IsRealCondition(ctx.CurIndex, instructions, isAsyncStateMachine)) //is real condition's branch?
                return;
            #endregion
            
            //ELSE/JUMP
            try
            {
                var ifInst = ifStack.Pop();

                //data
                var pairedCode = ifInst.OpCode.Code;
                var crossType = pairedCode is Code.Brfalse or Code.Brfalse_S ? CrossPointType.Else : CrossPointType.If;
                probData = _probeHelper.GetProbeData(ctx, crossType);

                var firstProbeInst = GetFirstInstruction(probData); //probe's first instruction
                FixFinallyEnd(instr, firstProbeInst, exceptionHandlers); //need fix statement boundaries for potential tr/finally 

                var oldJumpTarget = instructions[ctx.CurIndex + 1]; //where the code jumped earlier
                ReplaceJump(oldJumpTarget, firstProbeInst, jumpers); //...we change it to our first instruction

                processor.InsertBefore(oldJumpTarget, firstProbeInst);
                processor.InsertBefore(oldJumpTarget, call);
                ctx.IncrementIndex(2);
                
                needBreak = true;
            }
            catch (Exception exx)
            {
                //log...
            }
        }
    }
}

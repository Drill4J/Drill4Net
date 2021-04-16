using System;
using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class NonConditionBranchHandler : AbstractBaseHandler
    {
        public NonConditionBranchHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_NONCONDITIONAL, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var treeType = ctx.Type;

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
            var ifStack = ctx.IfStack;
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
                var firstProbeInst = Register(ctx, crossType);

                //correction
                FixFinallyEnd(instr, firstProbeInst, exceptionHandlers); //need fix statement boundaries for potential tr/finally 
                var oldJumpTarget = instructions[ctx.CurIndex + 1]; //where the code jumped earlier
                ReplaceJumps(oldJumpTarget, firstProbeInst, ctx); //...we change it to our first instruction

                //injection
                processor.InsertBefore(oldJumpTarget, firstProbeInst);
                processor.InsertBefore(oldJumpTarget, call);
                ctx.CorrectIndex(2);
                
                needBreak = true;
            }
            catch (Exception exx)
            {
                //log...
            }
        }
    }
}

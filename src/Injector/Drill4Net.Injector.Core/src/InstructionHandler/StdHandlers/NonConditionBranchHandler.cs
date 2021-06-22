using System;
using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler for the cross-point of the nonconditional instruction's jump (branch)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public class NonConditionBranchHandler : AbstractBaseHandler
    {
        public NonConditionBranchHandler(AbstractProbeHelper probeHelper) :
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_NONCONDITIONAL, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;
            var lastOp = ctx.LastOperation;

            var source = ctx.Method.Source;
            var isAsyncStateMachine = source.IsAsyncStateMachine;
            var compilerInstructions = ctx.CompilerInstructions;
            var exceptionHandlers = ctx.ExceptionHandlers;
            var ifStack = ctx.IfStack;
            var proxyMethRef = ctx.AssemblyCtx.ProxyMethRef;
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);
            #endregion
            #region Checks
            if (code != Code.Br && code != Code.Br_S)
                return false;
            if (!ifStack.Any())
                return false;
            if (IsNextReturn(ctx.CurIndex, instructions, lastOp))
                return false;
            if (!isAsyncStateMachine && IsCompilerGeneratedBranch(ctx.CurIndex, instructions, compilerInstructions))
                return false;
            if (!IsRealCondition(ctx.CurIndex, instructions)) //is real condition's branch?
                return false;
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
                FixFinallyEnd(instr, firstProbeInst, exceptionHandlers); //need fix the statement boundaries for the potential try/finally 
                var oldJumpTarget = instructions[ctx.CurIndex + 1]; //where the code jumped earlier
                ReplaceJumps(oldJumpTarget, firstProbeInst, ctx); //...we change it to our first instruction

                //injection
                processor.InsertBefore(oldJumpTarget, firstProbeInst);
                processor.InsertBefore(oldJumpTarget, call);
                ctx.CorrectIndex(2);

                needBreak = true;
                return true;
            }
            catch (Exception exx)
            {
                //log...

                return false;
            }
        }
    }
}

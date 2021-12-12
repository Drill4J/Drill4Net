using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Branch" type
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public class BranchHandler : AbstractBaseHandler
    {
        public BranchHandler(AbstractProbeHelper probeHelper):
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void PostprocessConcrete(MethodContext ctx)
        {
            if (ctx.BusinessInstructions.Count == 0)
                return;
            //
            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var jumpers = ctx.BusinessInstructions
                .Where(a => ctx.Jumpers.Contains(a) && !ctx.Switches.Contains(a))
                .OrderBy(a => a.Offset);

            int lastProcInd = -1;
            foreach (var instr in jumpers)
            {
                var ind = instructions.IndexOf(instr);

                #region Check
                if (!IsRealCondition(ind, ctx))
                {
                    if (lastProcInd == -1)
                        continue;

                    //check whether it is necessary to create a new block because of the jump
                    //to the code after the last processed instruction
                    var anchorExists = false;
                    for (var i = lastProcInd + 1; i < ind - 1; i++)
                    {
                        if (ctx.Anchors.Contains(instructions[i]))
                        {
                            anchorExists = true;
                            break;
                        }
                    }
                    if (!anchorExists)
                        continue;
                }
                #endregion

                //data
                int origInd = ctx.OrigInstructions.IndexOf(instr);
                var ldstr = Register(ctx, CrossPointType.Branch, origInd);
                var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

                //correction
                var prev = MoveSkippingNops(ind, false, ctx);
                var isPrevBr = prev.OpCode.Code is Code.Br or Code.Br_S;
                var emtyBlock = instr.OpCode.FlowControl == FlowControl.Branch && isPrevBr; //br.s direct to br.s
                if (emtyBlock) //because we've shifted the anchor
                    ReplaceJumps(instr, ldstr, ctx);

                FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
                ctx.CorrectIndex(2);

                //injection
                processor.InsertBefore(instr, ldstr);
                processor.InsertBefore(instr, call);

                lastProcInd = ind;
            }
        }

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            return false;
        }
    }
}

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
            var jumpers = ctx.BusinessInstructions.Where(a => ctx.Jumpers.Contains(a) &&
                                                              !ctx.Switches.Contains(a));

            foreach (var instr in jumpers)
            {
                var ind = instructions.IndexOf(instr);
                if (!IsRealCondition(ind, ctx))
                    continue;

                //data
                int origInd = ctx.OrigInstructions.IndexOf(instr);
                var ldstr = Register(ctx, CrossPointType.Branch, origInd);
                var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

                //correction
                var prev = MoveSkippingNops(ind, false, ctx);
                var isPrevBr = prev.OpCode.Code is Code.Br or Code.Br_S;
                var emtyBlock = instr.OpCode.FlowControl == FlowControl.Branch && isPrevBr; //br.s -> br.s
                if (emtyBlock)
                    ReplaceJumps(instr, ldstr, ctx);

                FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
                ctx.CorrectIndex(2);

                //injection
                processor.InsertBefore(instr, ldstr);
                processor.InsertBefore(instr, call);
            }
        }

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            return false;
        }
    }
}

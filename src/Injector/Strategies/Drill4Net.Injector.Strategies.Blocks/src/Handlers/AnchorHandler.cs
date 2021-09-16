using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Anchor" type (instructions which are jumpers
    /// or the targets for another instruction-jumpers or call business-methods, and thus dividing
    /// IL code to the blocks)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractSimpleHandler" />
    public class AnchorHandler : AbstractSimpleHandler
    {
        public AnchorHandler(AbstractProbeHelper probeHelper) :
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, CrossPointType.Anchor, probeHelper, false)
        {
        }

        /************************************************************************************/

        protected override bool IsCondition(MethodContext ctx)
        {
            return false;
        }

        protected override void PostprocessConcrete(MethodContext ctx)
        {
            //in fact, will process not processed instructions
            var ret = ctx.Instructions.Last();
            var instrs = ctx.BusinessInstructions
                .Where(a => ctx.Anchors.Contains(a) &&
                            !ctx.Processed.Contains(a) &&
                            !ctx.Cycles.Contains(a) &&
                            !ctx.ReplacedJumps.ContainsKey(a) &&
                            !IsPreviousBad(a) &&
                            a != ret
                        );
            foreach (var instr in instrs)
            {
                var ind = ctx.Instructions.IndexOf(instr);
                //TODO: instead such inefficient check better immediately to exclude the bad branches in ctx.Anchors
                var prev = SkipNops(ind, false, ctx);
                var prevInd = ctx.Instructions.IndexOf(prev);
                if (!IsRealCondition(prevInd, ctx))
                    continue;
                ctx.SetPosition(ind);
                ProcessInstruction(ctx);
            }
        }

        private bool IsPreviousBad(Instruction instr)
        {
            return instr.Previous is { OpCode: { Code: Code.Leave or Code.Leave_S or Code.Br or Code.Br_S } };
        }
    }
}

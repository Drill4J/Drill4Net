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
        private readonly bool _ignoreCycles;

        /************************************************************************************/

        public AnchorHandler(AbstractProbeHelper probeHelper, bool ignoreCycles):
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, CrossPointType.Anchor, probeHelper, false)
        {
            _ignoreCycles = ignoreCycles;
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
                if (_ignoreCycles && !CheckOnIfElseProcessed(ctx, instr))
                {
                    continue;
                }
                //
                ctx.SetPosition(ind);
                ProcessInstruction(ctx);
            }
        }

        //Guano: ?
        private bool CheckOnIfElseProcessed(MethodContext ctx, Instruction instr)
        {
            //TODO: caching!!!
            foreach (var cur in ctx.Jumpers)
            {
                Instruction[] operands;
                if (cur.Operand is Instruction instruction)
                    operands = new[] { instruction };
                else
                    operands = (Instruction[])cur.Operand; //'switch' statement
                //
                foreach (var operand in operands)
                {
                    var anchor = operand;
                    if (ctx.StartingInjectInstructions.Contains(anchor) && ctx.ReplacedJumps.ContainsKey(anchor))
                        anchor = ctx.ReplacedJumps[anchor];
                    if (anchor != instr)
                        continue;
                    //
                    var jumpFlow = cur.OpCode.FlowControl;
                    if (jumpFlow == FlowControl.Cond_Branch)
                        return false;
                    if (jumpFlow == FlowControl.Branch && cur.Previous is { OpCode: { FlowControl: FlowControl.Cond_Branch } })
                        return false;
                }
            }
            return true;
        }

        private bool IsPreviousBad(Instruction instr)
        {
            return instr.Previous is { OpCode: { Code: Code.Leave or Code.Leave_S or Code.Br or Code.Br_S } };
        }
    }
}

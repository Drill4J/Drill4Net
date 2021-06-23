using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using System.Linq;

namespace Drill4Net.Injector.Core
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
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, CrossPointType.Anchor, probeHelper)
        {
            _ignoreCycles = ignoreCycles;
        }

        /************************************************************************************/

        protected override bool IsCondition(MethodContext ctx)
        {
            var instr = ctx.CurInstruction;
            var flow = instr.OpCode.FlowControl;
            //
            if (!ctx.Anchors.Contains(instr))
                return false;
            if (ctx.CompilerInstructions.Contains(instr))
                return false;
            if (flow is not FlowControl.Next and not FlowControl.Call)
                return false;
            if (instr.Previous is { OpCode: { Code: Code.Leave or Code.Leave_S } })
                return false;
            if (ctx.Processed.Contains(instr)) //it can already be processed
                return false;

            //we need use only 'pure jumps', for example, 'goto' statement,
            //not if/else or cycles jumps, there are special handlers for these purposes
            if (_ignoreCycles)
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
                        var jumpFlow = cur.OpCode.FlowControl;
                        if (jumpFlow == FlowControl.Cond_Branch)
                            return false;
                        if (jumpFlow == FlowControl.Branch && cur.Previous is { OpCode: { FlowControl: FlowControl.Cond_Branch } })
                            return false;
                    }
                }
            }
            //
            return true;
        }

        protected override void PostprocessConcrete(MethodContext ctx)
        {
            //in fact, will process not processed instructions
            var ret = ctx.Instructions.Last();
            foreach (var instr in ctx.BusinessInstructions
                                    .Where(a => ctx.Anchors.Contains(a) &&
                                                !ctx.Processed.Contains(a) &&
                                                !ctx.ReplacedJumps.ContainsKey(a) &&
                                                a != ret
                                           ))
            {
                var ind = ctx.Instructions.IndexOf(instr);
                ctx.SetPosition(ind);
                ProcessInstruction(ctx);
            }
        }
    }
}

using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class AnchorHandler : AbstractSimpleHandler
    {
        private bool _ignoreCycles;
        
        /************************************************************************************/
        
        public AnchorHandler(AbstractProbeHelper probeHelper, bool ignoreCycles) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, CrossPointType.Anchor, probeHelper)
        {
            _ignoreCycles = ignoreCycles;
        }

        /************************************************************************************/

        protected override bool IsCondition(InjectorContext ctx)
        {
            var instr = ctx.Instructions[ctx.CurIndex];
            var flow = instr.OpCode.FlowControl;
            //
            if (flow is not FlowControl.Next and not FlowControl.Call)
                return false;
            var prevCode = instr.Previous.OpCode.Code;
            if(prevCode is Code.Leave or Code.Leave_S)
                return false;
            if(ctx.CompilerInstructions.Contains(instr))
                return false;
            if (!ctx.Anchors.Contains(instr))
                return false;
            
            //we need use only 'pure jumps', for example, 'goto' statement,
            //not if/else or cycles jumps, there are special handlers for these purposes
            if (_ignoreCycles)
            {
                foreach (var cur in ctx.Jumpers)
                {
                    var anchor = (Instruction)cur.Operand;
                    if (ctx.FirstInjectInstructions.Contains(anchor) && ctx.ReplacedJumps.ContainsKey(anchor))
                        anchor = ctx.ReplacedJumps[anchor];
                    if (anchor != instr)
                         continue;
                    var jumpFlow = cur.OpCode.FlowControl;
                    if (jumpFlow == FlowControl.Cond_Branch)
                        return false;
                    if (jumpFlow == FlowControl.Branch && cur.Previous.OpCode.FlowControl == FlowControl.Cond_Branch)
                        return false;
                }
            }
            //
            return true;
        }
    }
}

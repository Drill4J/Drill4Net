using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class AnchorHandler : AbstractSimpleHandler
    {
        public AnchorHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, CrossPointType.Anchor, probeHelper)
        {
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
            //
            return true;
        }
        
    }
}

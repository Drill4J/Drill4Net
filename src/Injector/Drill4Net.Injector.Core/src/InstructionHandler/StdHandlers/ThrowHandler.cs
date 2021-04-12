using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ThrowHandler : AbstractSimpleHandler
    {
        public ThrowHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_THROW, CrossPointType.Throw, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override bool IsCondition(InjectorContext ctx)
        {
            var instr = ctx.Instructions[ctx.CurIndex];
            return instr.OpCode.FlowControl == FlowControl.Throw;
        }
    }
}

using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler of the cross-point for the "Throw" type (throw instruction)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractSimpleHandler" />
    public class ThrowHandler : AbstractSimpleHandler
    {
        public ThrowHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_THROW, CrossPointType.Throw, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override bool IsCondition(MethodContext ctx)
        {
            var instr = ctx.Instructions[ctx.CurIndex];
            return instr.OpCode.FlowControl == FlowControl.Throw;
        }
    }
}

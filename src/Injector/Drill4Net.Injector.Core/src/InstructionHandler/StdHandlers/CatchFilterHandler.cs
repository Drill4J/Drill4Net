using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using static Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler for the cross-point of the "CatchFilter" type (Catch part of the try/ctach construction)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractSimpleHandler" />
    public class CatchFilterHandler : AbstractSimpleHandler
    {
        public CatchFilterHandler(AbstractProbeHelper probeHelper):
            base(INSTRUCTION_HANDLER_CATCH_FILTER, CrossPointType.CatchFilter, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override bool IsCondition(MethodContext ctx)
        {
            var instr = ctx.Instructions[ctx.CurIndex];
            return instr.OpCode.Code == Code.Endfilter;
        }
    }
}

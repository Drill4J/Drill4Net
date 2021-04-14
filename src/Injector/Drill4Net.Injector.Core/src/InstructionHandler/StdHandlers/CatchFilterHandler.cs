using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    public class CatchFilterHandler : AbstractSimpleHandler
    {
        public CatchFilterHandler(AbstractProbeHelper probeHelper): 
            base(C.INSTRUCTION_HANDLER_CATCH_FILTER, CrossPointType.CatchFilter, probeHelper)
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

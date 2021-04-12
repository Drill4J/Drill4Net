using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    public class CallHandler : AbstractSimpleHandler
    {
        public CallHandler(AbstractProbeHelper probeHelper): 
            base(C.INSTRUCTION_HANDLER_CALL, CrossPointType.Call, probeHelper)
        {
        }
        
        /**************************************************************************/

        protected override bool IsCondition(InjectorContext ctx)
        {
            var instr = ctx.Instructions[ctx.CurIndex];
            return instr.OpCode.Code is Code.Call or Code.Calli or Code.Callvirt;
        }
    }
}
using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class EnterHandler : AbstractBaseHandler
    {
        public EnterHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_CONDITIONAL, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void StartMethodConcrete(MethodContext ctx)
        {
            if (ctx.IsStrictEnterReturn || !ctx.Instructions.Any())
                return;

            //data
            var ldstrEntering = Register(ctx, CrossPointType.Enter, 0);//exactly 0 !

            //injection
            var firstOp = ctx.Instructions[0];
            var proxyMethRef = ctx.TypeCtx.AssemblyCtx.ProxyMethRef;
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);

            ctx.Processor.InsertBefore(firstOp, ldstrEntering);
            ctx.Processor.InsertBefore(firstOp, call);
        }

        protected override void HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
        }
    }
}

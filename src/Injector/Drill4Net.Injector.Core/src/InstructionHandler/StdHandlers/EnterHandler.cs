using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Enter" type (enter into method)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public class EnterHandler : AbstractBaseHandler
    {
        public EnterHandler(AbstractProbeHelper probeHelper):
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_CONDITIONAL, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void PreprocessConcrete(MethodContext ctx)
        {
            if (ctx.IsStrictEnterReturn || !ctx.Instructions.Any())
                return;

            //data
            var ldstrEntering = Register(ctx, CrossPointType.Enter, 0); //exactly 0

            //injection
            var firstOp = ctx.Instructions[0];
            var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

            ctx.Processor.InsertBefore(firstOp, ldstrEntering);
            ctx.Processor.InsertBefore(firstOp, call);
        }

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            return false;
        }
    }
}

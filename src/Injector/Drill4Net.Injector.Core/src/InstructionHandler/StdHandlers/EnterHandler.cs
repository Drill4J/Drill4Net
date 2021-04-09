using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    public class EnterHandler : AbstractInstructionHandler
    {
        public EnterHandler() : base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_CONDITIONAL)
        {
        }

        /*****************************************************************************/

        protected override void StartMethodConcrete(InjectorContext ctx)
        {
            if (ctx.IsStrictEnterReturn || !ctx.Instructions.Any())
                return;

            //data
            var probData = _probeHelper.GetProbeData(ctx.TreeMethod, ctx.ModuleName, CrossPointType.Enter, 0);
            var ldstrEntering = GetFirstInstruction(probData);

            //injection
            var firstOp = ctx.Instructions[0];
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);

            ctx.Processor.InsertBefore(firstOp, ldstrEntering);
            ctx.Processor.InsertBefore(firstOp, call);
        }

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            needBreak = false;
        }
    }
}

using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Jumper" type
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public class JumperHandler : AbstractBaseHandler
    {
        public JumperHandler(AbstractProbeHelper probeHelper) :
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_JUMPER, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void PostprocessConcrete(MethodContext ctx)
        {
            if (!ctx.BusinessInstructions.Any())
                return;
            //
            var processor = ctx.Processor;
            foreach (var instr in ctx.BusinessInstructions.Where(a => ctx.Jumpers.Contains(a)))
            {
                var ind = ctx.Instructions.IndexOf(instr);
                var ldstr = Register(ctx, CrossPointType.Jumper, ind);
                var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

                //correction
                FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
                ctx.CorrectIndex(2);

                //injection
                processor.InsertBefore(instr, ldstr);
                processor.InsertBefore(instr, call);
            }
        }

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            return false;
        }
    }
}

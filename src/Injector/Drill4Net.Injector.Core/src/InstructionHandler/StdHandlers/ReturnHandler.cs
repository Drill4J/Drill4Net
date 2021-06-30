using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Return" type (return from the method)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public class ReturnHandler : AbstractBaseHandler
    {
        public ReturnHandler(AbstractProbeHelper probeHelper):
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_RETURN, probeHelper)
        {    
        }

        /**************************************************************************************/

        protected override void PreprocessConcrete(MethodContext ctx)
        {
            if (ctx.IsStrictEdgeCrosspoints)
                return;

            //init
            var processor = ctx.Processor;
            var instr = ctx.CurInstruction;
            var proxyMethRef = ctx.AssemblyCtx.ProxyMethRef;
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);

            //data
            var ind = ctx.OrigSize - 1;
            var returnInst = Register(ctx, CrossPointType.Return, ind, false); //as object it must be only one
            ctx.LastOperation = returnInst;

            //correction
            FixFinallyEnd(instr, returnInst, ctx.ExceptionHandlers);
            ReplaceJumps(instr, returnInst, ctx);

            //injection
            processor.InsertBefore(instr, returnInst);
            processor.InsertBefore(instr, call);
            ctx.CorrectIndex(2);
        }
        
        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            return false;
        }
    }
}

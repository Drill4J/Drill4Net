using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ReturnHandler : AbstractBaseHandler
    {
        public ReturnHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_RETURN, probeHelper)
        {          
        }

        /**************************************************************************************/

        protected override void StartMethodConcrete(MethodContext ctx)
        {
            if (ctx.IsStrictEnterReturn)
                return;
            
            //init
            var processor = ctx.Processor;
            var instr = ctx.CurInstruction;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            
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
        
        protected override void HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
        }
    }
}

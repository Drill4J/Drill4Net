using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Branch" type
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public class BranchHandler : AbstractBaseHandler
    {
        public BranchHandler(AbstractProbeHelper probeHelper):
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH, probeHelper)
        {
        }

        /*****************************************************************************/

        protected override void PostprocessConcrete(MethodContext ctx)
        {
            if (ctx.BusinessInstructions.Count == 0)
                return;
            //
            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var jumpers = ctx.BusinessInstructions.Where(a => ctx.Jumpers.Contains(a));
            foreach (var instr in jumpers)
            {
                if (ctx.Processed.Contains(instr)) //it's needed here
                    continue;

                var anchor = instr.Operand as Instruction;
                if (anchor?.OpCode.Code is Code.Br or Code.Br_S) //additional after-branch
                {
                    var origInd = ctx.OrigInstructions.IndexOf(anchor);
                    var ldstr2 = Register(ctx, CrossPointType.Branch, origInd);
                    var call2 = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

                    //correction
                    FixFinallyEnd(anchor, ldstr2, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
                    ctx.CorrectIndex(2);

                    //injection
                    processor.InsertBefore(anchor, ldstr2);
                    processor.InsertBefore(anchor, call2);

                    instr.Operand = ldstr2;
                    ctx.RegisterProcessed(anchor);
                }
                else //normal before-branch
                {
                    var ind = instructions.IndexOf(instr);

                    //check for too short jump
                    var prev = SkipNops(ind, false, ctx);
                    var prevInd = instructions.IndexOf(prev);
                    if (!IsRealCondition(prevInd, ctx))
                        continue;

                    var origInd = ctx.OrigInstructions.IndexOf(instr);
                    var ldstr = Register(ctx, CrossPointType.Branch, origInd);
                    var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

                    //correction
                    FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
                    ctx.CorrectIndex(2);

                    //injection
                    processor.InsertBefore(instr, ldstr);
                    processor.InsertBefore(instr, call);
                }
            }
        }

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            return false;
        }
    }
}

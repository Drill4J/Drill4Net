using Drill4Net.Profiling.Tree;
using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Core
{
    public class AnchorHandler : AbstractInstructionHandler
    {
        public AnchorHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, probeHelper)
        {
        }

        /************************************************************************************/

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            
            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var flow = opCode.FlowControl;
            
            var compilerInstructions = ctx.CompilerInstructions;
            var jumpers = ctx.Jumpers;
            var anchors = ctx.Anchors;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion
            #region Checks
            if (flow != FlowControl.Next && flow != FlowControl.Call)
                return;
            var prevCode = instr.Previous.OpCode.Code;
            if(prevCode is Code.Leave or Code.Leave_S)
                return;
            if(compilerInstructions.Contains(instr))
                return;
            if (!anchors.Contains(instr))
                return;
            #endregion

            //data
            var probData = GetProbeData(ctx);
            var ldstr = GetFirstInstruction(probData);
            
            //correction
            FixFinallyEnd(instr, ldstr, exceptionHandlers);
            ReplaceJump(instr, ldstr, jumpers);

            //injection
            processor.InsertBefore(instr, ldstr);
            processor.InsertBefore(instr, call);
            ctx.IncrementIndex(2);
            
            needBreak = true;
        }
        
        protected virtual string GetProbeData(InjectorContext ctx)
        {
            return _probeHelper.GetProbeData(ctx, CrossPointType.Anchor, ctx.CurIndex);
        }
    }
}

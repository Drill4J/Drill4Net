using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ThrowHandler : AbstractInstructionHandler
    {
        public ThrowHandler() : base(InjectorCoreConstants.INSTRUCTION_HANDLER_THROW)
        {
        }

        /*****************************************************************************/

        protected override void HandleRequestConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var moduleName = ctx.ModuleName;
            var treeFunc = ctx.TreeMethod;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            //var code = opCode.Code;
            var flow = opCode.FlowControl;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var jumpers = ctx.Jumpers;
            string probData;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            //THROW
            if (flow == FlowControl.Throw)
            {
                probData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.Throw, ctx.CurIndex);
                var throwInst = GetInstruction(probData);
                FixFinallyEnd(instr, throwInst, exceptionHandlers);
                ReplaceJump(instr, throwInst, jumpers);
                processor.InsertBefore(instr, throwInst);
                processor.InsertBefore(instr, call);
                ctx.IncrementIndex(2);
            }
        }
    }
}

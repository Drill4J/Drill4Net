using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class CatchFilterHandler : AbstractInstructionHandler
    {
        public CatchFilterHandler(): base(InjectorCoreConstants.INSTRUCTION_HANDLER_CATCH_FILTER)
        {
        }

        /*****************************************************************************/

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var moduleName = ctx.ModuleName;
            var treeFunc = ctx.TreeMethod;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            //CATCH FILTER
            if (code != Code.Endfilter) 
                return;

            var probData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.CatchFilter, ctx.CurIndex);
            var ldstr = GetFirstInstruction(probData);
            FixFinallyEnd(instr, ldstr, exceptionHandlers);
            processor.InsertBefore(instr, ldstr);
            processor.InsertBefore(instr, call);
            ctx.IncrementIndex(2);
        }
    }
}

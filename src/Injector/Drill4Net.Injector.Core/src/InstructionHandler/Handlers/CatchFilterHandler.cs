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
            var code = opCode.Code;

            var exceptionHandlers = ctx.ExceptionHandlers;
            string probData;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            //CATCH FILTER
            if (code == Code.Endfilter)
            {
                probData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.CatchFilter, ctx.CurIndex);
                var ldstrFlt = GetInstruction(probData);
                FixFinallyEnd(instr, ldstrFlt, exceptionHandlers);
                processor.InsertBefore(instr, ldstrFlt);
                processor.InsertBefore(instr, call);
                ctx.IncrementIndex(2);
            }
        }
    }
}

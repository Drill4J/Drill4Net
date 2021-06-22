using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Abstract conditional branch handler for the IL code's instructions
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public abstract class AbstractConditionBranchHandler : AbstractBaseHandler
    {
        protected AbstractConditionBranchHandler(AbstractProbeHelper probeHelper):
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_CONDITIONAL, probeHelper)
        {
        }

        /*******************************************************************************************/

        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;

            var treeFunc = ctx.Method;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;
            var flow = opCode.FlowControl;

            var methodSource = treeFunc.Source;
            var isAsyncStateMachine = methodSource.IsAsyncStateMachine;
            var isEnumeratorMoveNext = methodSource.IsEnumeratorMoveNext;

            var compilerInstructions = ctx.CompilerInstructions;
            var isBrFalse = code is Code.Brfalse or Code.Brfalse_S; //TODO: add another branch codes? Hmm...
            #endregion
            #region Checks
            if (flow != FlowControl.Cond_Branch)
                return false;

            #region 'Using' statement
            //check for 'using' statement (compiler generated Try/Finally with If-checking)
            //There is a possibility that a very similar construction from the business code
            //may be omitted if the programmer directly implemented Try/Finally with a check
            //and a Dispose() call, instead of the usual 'using', although it is unlikely
            if (ctx.CurIndex > 2 && ctx.CurIndex < instructions.Count - 2)
            {
                var prev2 = instructions[ctx.CurIndex - 2].OpCode.Code;
                if (prev2 == Code.Throw)
                    return false;
                var isWasTry = prev2 == Code.Leave || prev2 == Code.Leave_S;
                if (isWasTry)
                {
                    var b = instructions[ctx.CurIndex + 2];
                    var isDispose = (b.Operand as MemberReference)?.FullName?.EndsWith("IDisposable::Dispose()") == true;
                    if (isDispose)
                        return false;
                }
            }
            #endregion
            #region Is this business code?
            if (!isAsyncStateMachine && !isEnumeratorMoveNext && IsCompilerGeneratedBranch(ctx.CurIndex, instructions, compilerInstructions))
                return false;
            if (!IsRealCondition(ctx.CurIndex, instructions))
                return false;
            #endregion
            #region Monitor/lock
            var operand = instr.Operand as Instruction;
            if (isBrFalse && operand is {OpCode: {Code: Code.Endfinally}})
            {
                var endFinInd = instructions.IndexOf(operand);
                var prevInstr = SkipNop(endFinInd, false, instructions);
                var operand2 = prevInstr.Operand as MemberReference;
                if (operand2?.FullName?.Equals("System.Void System.Threading.Monitor::Exit(System.Object)") == true)
                    return false;
            }
            #endregion
            #endregion

            //IF, FOR/SWITCH
            return ProcessConditionInstruction(ctx, out needBreak);
        }

        protected abstract bool ProcessConditionInstruction(MethodContext ctx, out bool needBreak);
    }
}

using System;
using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class NonConditionBranchHandler : AbstractInstructionHandler
    {
        public NonConditionBranchHandler(): base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_NONCONDITIONAL)
        {
        }

        /*****************************************************************************/

        protected override void HandleRequestConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var moduleName = ctx.ModuleName;
            var treeType = ctx.TreeType;
            var treeFunc = ctx.TreeMethod;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;
            var flow = opCode.FlowControl;
            var lastOp = ctx.LastOperation;

            var typeSource = treeType.SourceType;
            var isAsyncStateMachine = typeSource.IsAsyncStateMachine;
            var compilerInstructions = ctx.CompilerInstructions;
            var exceptionHandlers = ctx.ExceptionHandlers;
            var jumpers = ctx.Jumpers;
            var ifStack = ctx.IfStack;
            string probData;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            //ELSE/JUMP
            if (flow == FlowControl.Branch && (code == Code.Br || code == Code.Br_S)) //also may be Code.Leave
            {
                if (!ifStack.Any())
                    return;
                if (IsNextReturn(ctx.CurIndex, instructions, lastOp))
                    return;
                if (!isAsyncStateMachine && IsCompilerGeneratedBranch(ctx.CurIndex, instructions, compilerInstructions))
                    return;
                if (!IsRealCondition(ctx.CurIndex, instructions, isAsyncStateMachine)) //is real condition's branch?
                    return;
                //
                var ifInst = ifStack.Pop();
                var pairedCode = ifInst.OpCode.Code;
                var crossType = pairedCode == Code.Brfalse || pairedCode == Code.Brfalse_S ? CrossPointType.Else : CrossPointType.If;
                probData = _probeHelper.GetProbeData(treeFunc, moduleName, crossType, ctx.CurIndex);
                var elseInst = GetInstruction(probData);

                try
                {
                    var instr2 = instructions[ctx.CurIndex + 1];
                    FixFinallyEnd(instr, elseInst, exceptionHandlers);
                    ReplaceJump(instr2, elseInst, jumpers);
                    processor.InsertBefore(instr2, elseInst);
                    processor.InsertBefore(instr2, call);
                    ctx.IncrementIndex(2);
                }
                catch (Exception exx)
                { }
            }
        }

    }
}

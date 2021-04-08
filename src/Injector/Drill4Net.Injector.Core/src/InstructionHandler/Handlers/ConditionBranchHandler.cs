using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ConditionBranchHandler : AbstractInstructionHandler
    {
        public ConditionBranchHandler():  base(InjectorCoreConstants.INSTRUCTION_HANDLER_BRANCH_CONDITIONAL)
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

            var typeSource = treeType.SourceType;
            var methodSource = treeFunc.SourceType;

            var isAsyncStateMachine = typeSource.IsAsyncStateMachine;
            var isEnumeratorMoveNext = methodSource.IsEnumeratorMoveNext;

            var compilerInstructions = ctx.CompilerInstructions;
            var processed = ctx.Processed;
            var jumpers = ctx.Jumpers;
            var ifStack = ctx.IfStack;

            var crossType = CrossPointType.Unset;
            string probData;

            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            #endregion

            //IF, FOR/SWITCH
            if (flow == FlowControl.Cond_Branch)
            {
                #region 'Using' statement
                //check for 'using' statement (compiler generated Try/Finally with If-checking)
                //There is a possibility that a very similar construction from the business code
                //may be omitted if the programmer directly implemented Try/Finally with a check
                //and a Dispose() call, instead of the usual 'using', although it is unlikely
                if (ctx.CurIndex > 2 && ctx.CurIndex < instructions.Count - 2)
                {
                    var prev2 = instructions[ctx.CurIndex - 2].OpCode.Code;
                    if (prev2 == Code.Throw)
                        return;
                    var isWasTry = prev2 == Code.Leave || prev2 == Code.Leave_S;
                    if (isWasTry)
                    {
                        var b = instructions[ctx.CurIndex + 2];
                        var isDispose = (b.Operand as MemberReference)?.FullName?.EndsWith("IDisposable::Dispose()") == true;
                        if (isDispose)
                            return;
                    }
                }
                #endregion

                if (!isAsyncStateMachine && !isEnumeratorMoveNext && IsCompilerGeneratedBranch(ctx.CurIndex, instructions, compilerInstructions))
                    return;
                if (!IsRealCondition(ctx.CurIndex, instructions, isAsyncStateMachine))
                    return;
                //
                var isBrFalse = code == Code.Brfalse || code == Code.Brfalse_S; //TODO: add another branch codes? Hmm...

                #region Monitor/lock
                var operand = instr.Operand as Instruction;
                if (isBrFalse && operand != null && operand.OpCode.Code == Code.Endfinally)
                {
                    var endFinInd = instructions.IndexOf(operand);
                    var prevInstr = SkipNop(endFinInd, false, instructions);
                    var operand2 = prevInstr.Operand as MemberReference;
                    if (operand2?.FullName?.Equals("System.Void System.Threading.Monitor::Exit(System.Object)") == true)
                        return;
                }
                #endregion
                #region Operators: while/for, do
                if (operand != null && operand.Offset > 0 && instr.Offset > operand.Offset)
                {
                    if (isEnumeratorMoveNext)
                    {
                        var prevRef = (instr.Previous.Operand as MemberReference).FullName;
                        if (prevRef?.Contains("::MoveNext()") == true)
                            return;
                    }
                    //
                    var ind = instructions.IndexOf(operand); //inefficient, but it will be rarely...
                    var prevOperand = SkipNop(ind, false, instructions);
                    if (prevOperand.OpCode.Code == Code.Br || prevOperand.OpCode.Code == Code.Br_S) //for/while
                    {
                        probData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.Cycle, ctx.CurIndex);
                        var ldstrIf2 = GetInstruction(probData);
                        var targetOp = prevOperand.Operand as Instruction;
                        processor.InsertBefore(targetOp, ldstrIf2);
                        processor.InsertBefore(targetOp, call);
                        ctx.IncrementIndex(2);

                        probData = _probeHelper.GetProbeData(treeFunc, moduleName, CrossPointType.CycleEnd, ctx.CurIndex);
                        var ldstrIf3 = GetInstruction(probData);
                        var call1 = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
                        processor.InsertAfter(instr, call1);
                        processor.InsertAfter(instr, ldstrIf3);
                        ctx.IncrementIndex(2);
                    }
                    else //do
                    {
                        var back = instr.Operand;
                        var next = instr.Next;

                        // 1.
                        crossType = isBrFalse ? CrossPointType.Cycle : CrossPointType.CycleEnd;
                        probData = _probeHelper.GetProbeData(treeFunc, moduleName, crossType, ctx.CurIndex);
                        var ldstrIf = GetInstruction(probData);

                        var call1 = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
                        processor.InsertAfter(instr, call1);
                        processor.InsertAfter(instr, ldstrIf);
                        ctx.IncrementIndex(2);

                        //jump-1
                        var jump = Instruction.Create(OpCodes.Br_S, next);
                        processor.InsertAfter(call1, jump);
                        jumpers.Add(jump);
                        ctx.IncrementIndex(1);

                        // 2.
                        crossType = !isBrFalse ? CrossPointType.Cycle : CrossPointType.CycleEnd;
                        probData = _probeHelper.GetProbeData(treeFunc, moduleName, crossType, ctx.CurIndex);
                        var ldstrIf2 = GetInstruction(probData);

                        var call2 = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
                        processor.InsertAfter(jump, call2);
                        processor.InsertAfter(jump, ldstrIf2);
                        ctx.IncrementIndex(2);

                        //jump-2
                        var jump2 = Instruction.Create(OpCodes.Br, back as Instruction);
                        processor.InsertAfter(call2, jump2);
                        jumpers.Add(jump2);
                        ctx.IncrementIndex(1);

                        instr.Operand = ldstrIf2;
                    }
                    return;
                }
                #endregion
                #region Switch
                //Whether the 'if/else' condition branches or the 'switch' instruction will be
                //inserted in the IL code does not depend on the Framework version, but depends on
                //the number of source 'cases' (two 'cases' - > 'if/else' branches are formed,
                //and if more - 'switch'). This does not depend on having a 'default' statement,
                //exiting the function by 'return' directly from 'case', or the compiler option
                //'Optimize code'. The exception only one: 'case' with 'when' condition always
                //will be generated whole construction as 'if/else' branches for any framework
                //version. This also means that it is impossible to determine exactly what was
                //in the source code only from the IL code.

                ifStack.Push(instr);
                if (code == Code.Switch)
                {
                    for (var k = 0; k < ((Instruction[])instr.Operand).Length - 1; k++)
                        ifStack.Push(instr);
                    crossType = CrossPointType.Switch;
                }
                #endregion
                #region IF
                if (code == Code.Switch || instructions[ctx.CurIndex + 1].OpCode.FlowControl != FlowControl.Branch) //empty IF?
                {
                    if (crossType == CrossPointType.Unset)
                        crossType = isBrFalse ? CrossPointType.If : CrossPointType.Else;
                    probData = _probeHelper.GetProbeData(treeFunc, moduleName, crossType, ctx.CurIndex);
                    var ldstrIf = GetInstruction(probData);

                    //when inserting 'after', must set in desc order
                    processor.InsertAfter(instr, call);
                    processor.InsertAfter(instr, ldstrIf);
                    ctx.IncrementIndex(2);
                }
                #endregion
                #region 'Switch when()', etc
                var prev = operand?.Previous;
                if (prev == null || processed.Contains(prev))
                    return;
                var prevCode = prev.OpCode.Code;
                if (prevCode == Code.Br || prevCode == Code.Br_S) //need insert paired call
                {
                    //TODO: совместить с веткой ELSE/JUMP ?
                    crossType = crossType == CrossPointType.If ? CrossPointType.Else : CrossPointType.If;
                    var ind = instructions.IndexOf(operand);
                    probData = _probeHelper.GetProbeData(treeFunc, moduleName, crossType, ind);
                    var elseInst = GetInstruction(probData);

                    ReplaceJump(operand, elseInst, jumpers);
                    processor.InsertBefore(operand, elseInst);
                    processor.InsertBefore(operand, call);

                    processed.Add(prev);
                    if (operand.Offset < instr.Offset)
                        ctx.IncrementIndex(2);
                }
                #endregion
            }
        }
    }
}

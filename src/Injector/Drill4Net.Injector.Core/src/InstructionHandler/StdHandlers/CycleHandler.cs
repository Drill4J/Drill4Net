using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class CycleHandler : AbstractConditionBranchHandler
    {
        public CycleHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }

        /***********************************************************************************************/

        protected override void ProcessConditionInstruction(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var treeFunc = ctx.Method;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var methodSource = treeFunc.SourceType;
            var isEnumeratorMoveNext = methodSource.IsEnumeratorMoveNext;

            var jumpers = ctx.Jumpers;
            string probData;

            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            var isBrFalse = code is Code.Brfalse or Code.Brfalse_S; //TODO: add another branch codes? Hmm...
            #endregion
            #region Check
            var operand = instr.Operand as Instruction;
            if (operand is not { Offset: > 0 } || instr.Offset < operand.Offset)
                return;
            if (isEnumeratorMoveNext)
            {
                var prevRef = ((MemberReference)instr.Previous.Operand).FullName;
                if (prevRef?.Contains("::MoveNext()") == true)
                    return;
            }
            #endregion

            // Operators: while/for, do
            var ind = instructions.IndexOf(operand);
            var prevOperand = SkipNop(ind, false, instructions);
            if (prevOperand.OpCode.Code is Code.Br or Code.Br_S) //for/while
            {
                probData = _probeHelper.GetProbeData(ctx, CrossPointType.Cycle);
                var ldstrIf2 = GetFirstInstruction(ctx, probData);
                var targetOp = prevOperand.Operand as Instruction;
                processor.InsertBefore(targetOp, ldstrIf2);
                processor.InsertBefore(targetOp, call);
                ctx.IncrementIndex(2);

                probData = _probeHelper.GetProbeData(ctx, CrossPointType.CycleEnd);
                var ldstrIf3 = GetFirstInstruction(ctx, probData);
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
                var crossType = isBrFalse ? CrossPointType.Cycle : CrossPointType.CycleEnd;
                probData = _probeHelper.GetProbeData(ctx, crossType);
                var ldstrIf = GetFirstInstruction(ctx, probData);
                
                var call1 = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
                processor.InsertAfter(instr, call1);
                processor.InsertAfter(instr, ldstrIf);
                ctx.IncrementIndex(2);

                //jump-1
                var jump = Instruction.Create(OpCodes.Br_S, next);
                processor.InsertAfter(call1, jump);
                jumpers.Add(jump);
                ctx.IncrementIndex();

                // 2.
                crossType = !isBrFalse ? CrossPointType.Cycle : CrossPointType.CycleEnd;
                probData = _probeHelper.GetProbeData(ctx, crossType);
                var ldstrIf2 = GetFirstInstruction(ctx, probData);

                var call2 = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
                processor.InsertAfter(jump, call2);
                processor.InsertAfter(jump, ldstrIf2);
                ctx.IncrementIndex(2);

                //jump-2
                var jump2 = Instruction.Create(OpCodes.Br, back as Instruction);
                processor.InsertAfter(call2, jump2);
                jumpers.Add(jump2);
                ctx.IncrementIndex();

                instr.Operand = ldstrIf2;
            }
            
            needBreak = true;
        }
    }
}
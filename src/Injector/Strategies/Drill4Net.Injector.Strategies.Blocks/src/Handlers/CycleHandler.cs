using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Cycle" type (cycle constructions: do, while, etc)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractConditionBranchHandler" />
    public class CycleHandler : AbstractConditionBranchHandler
    {
        public CycleHandler(AbstractProbeHelper probeHelper): base(probeHelper)
        {
        }

        /******************************************************************************************/

        protected override bool ProcessConditionInstruction(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var treeFunc = ctx.Method;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = ctx.CurInstruction;
            var opCode = instr.OpCode;
            var code = opCode.Code;
            var proxyMethRef = ctx.AssemblyCtx.ProxyMethRef;

            var methodSource = treeFunc.Source;
            var isEnumeratorMoveNext = methodSource.IsEnumeratorMoveNext;
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);
            var isBrFalse = code is Code.Brfalse or Code.Brfalse_S; //TODO: add another branch codes? Hmm...
            #endregion
            #region Check
            var nextCode = instr.Next.OpCode.Code;
            if (nextCode == Code.Leave || nextCode == Code.Leave_S)
                return false;
            var operand = instr.Operand as Instruction;
            if (operand is not { Offset: > 0 } || instr.Offset < operand.Offset)
                return false;
            if (isEnumeratorMoveNext)
            {
                var prevRef = ((MemberReference)instr.Previous.Operand).FullName;
                if (prevRef?.Contains("::MoveNext()") == true)
                    return false;
            }
            #endregion

            ctx.Cycles.Add(instr.Next); //exactly here

            // Operators: while/for, do
            var ind = instructions.IndexOf(operand);
            var prevOperand = SkipNops(ind, false, ctx);
            if (prevOperand.OpCode.Code is Code.Br or Code.Br_S) //for/while
            {
                var ldstrIf2 = Register(ctx, CrossPointType.Cycle);
                var targetOp = (instr.Operand as Instruction)?.Previous; //no nop skipping
                if (targetOp != null) //hm... formal checking
                {
                    ctx.Cycles.Add(targetOp.Next); //exactly here

                    processor.InsertAfter(targetOp, call);
                    processor.InsertAfter(targetOp, ldstrIf2);
                    ctx.CorrectIndex(2);

                    instr.Operand = ldstrIf2;
                }

                var ldstrIf3 = Register(ctx, CrossPointType.CycleEnd);
                var call1 = Instruction.Create(OpCodes.Call, proxyMethRef);
                processor.InsertAfter(instr, call1);
                processor.InsertAfter(instr, ldstrIf3);
                ctx.CorrectIndex(2);
            }
            else //do
            {
                //its LocalId will be matched with paired instruction
                var crossType = isBrFalse ? CrossPointType.Cycle : CrossPointType.CycleEnd;
                var ldstrIf = Register(ctx, crossType);

                var call1 = Instruction.Create(OpCodes.Call, proxyMethRef);
                processor.InsertAfter(instr, call1);
                processor.InsertAfter(instr, ldstrIf);
                ctx.CorrectIndex(2);

                var jmpOperand = instr.Operand as Instruction;
                ctx.Cycles.Add(jmpOperand.Next); //exactly here

                crossType = isBrFalse ? CrossPointType.CycleEnd : CrossPointType.Cycle;
                var ldstrIf2 = Register(ctx, crossType);

                var call2 = Instruction.Create(OpCodes.Call, proxyMethRef);
                processor.InsertAfter(jmpOperand, call2);
                processor.InsertAfter(jmpOperand, ldstrIf2);
                ctx.CorrectIndex(2);

                instr.Operand = ldstrIf2;
            }

            needBreak = true;
            return true;
        }
    }
}
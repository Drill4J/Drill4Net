using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class IfElseHandler : AbstractConditionBranchHandler
    {
        public IfElseHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }

        /***********************************************************************************************/

        protected override void ProcessConditionInstruction(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var processed = ctx.Processed;
            var jumpers = ctx.Jumpers;
            var ifStack = ctx.IfStack;

            var crossType = CrossPointType.Unset;
            string probData;

            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            var isBrFalse = code is Code.Brfalse or Code.Brfalse_S; //TODO: add another branch codes? Hmm...
            #endregion
            #region Check
            //if pure if/else, but backward jump
            var operand = instr.Operand as Instruction;
            if (operand != null && instr.Offset > operand.Offset)
                return;
            //
            var prev = instr.Previous;
            var prevCode = prev.OpCode.Code;
            if (prevCode is Code.Callvirt or Code.Call)
            {
                var s = prev.ToString();
                if (s.EndsWith("get_IsCompleted()"))
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
                //data
                if (crossType == CrossPointType.Unset)
                    crossType = isBrFalse ? CrossPointType.If : CrossPointType.Else;
                var ldstr = Register(ctx, crossType);

                //injection
                processor.InsertAfter(instr, call);
                processor.InsertAfter(instr, ldstr);
                ctx.IncrementIndex(2);
                
                needBreak = true;
            }
            #endregion
            #region 'Switch when()', etc
            #region Checks
            prev = operand?.Previous;
            if (prev == null || processed.Contains(prev))
                return;
            prevCode = prev.OpCode.Code;
            if (prevCode is not Code.Br and not Code.Br_S)
                return;
            #endregion

            //data: need insert paired call
            crossType = crossType == CrossPointType.If ? CrossPointType.Else : CrossPointType.If;
            var ind = instructions.IndexOf(operand);
            var ldstr2 = Register(ctx, crossType, ind);

            //correction
            ReplaceJumps(operand, ldstr2, ctx);
            
            //injection
            processor.InsertBefore(operand, ldstr2);
            processor.InsertBefore(operand, call);
            if (operand.Offset < instr.Offset)
                ctx.IncrementIndex(2);
            
            //paired injected instructions are must be labeled as processed
            processed.Add(prev);
            
            needBreak = true;
            #endregion
        }
    }
}
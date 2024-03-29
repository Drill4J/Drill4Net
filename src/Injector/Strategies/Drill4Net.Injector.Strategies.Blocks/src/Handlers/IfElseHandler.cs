﻿using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// IL code's handler for the cross-point of the "IF/ELSE" type (conditional branch)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractConditionBranchHandler" />
    public class IfElseHandler : AbstractConditionBranchHandler
    {
        public IfElseHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }

        /***********************************************************************************************/

        protected override bool ProcessConditionInstruction(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = ctx.CurInstruction;
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var processedInstrs = ctx.Processed;
            var processed = false;
            var crossType = CrossPointType.Unset;

            var proxyMethRef = ctx.AssemblyCtx.ProxyMethRef;
            var call = Instruction.Create(OpCodes.Call, proxyMethRef);
            var isBrFalse = code is Code.Brfalse or Code.Brfalse_S; //TODO: add another branch codes? Hmm...
            #endregion
            #region Check
            //if pure if/else, but backward jump
            var operand = instr.Operand as Instruction;
            if (operand is {Offset: > 0} && instr.Offset > operand.Offset)
                return false;
            //
            var prev = instr.Previous;
            var prevCode = prev.OpCode.Code;
            if (prevCode is Code.Callvirt or Code.Call)
            {
                var s = prev.ToString();
                if (s.EndsWith("get_IsCompleted()"))
                    return false;
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

            if (code == Code.Switch)
                crossType = CrossPointType.Switch;
            #endregion
            #region IF
            if (code == Code.Switch || instructions[ctx.CurIndex + 1].OpCode.FlowControl != FlowControl.Branch) //empty IF?
            {
                //data
                if (crossType == CrossPointType.Unset)
                    crossType = isBrFalse ? CrossPointType.If : CrossPointType.Else;
                var ldstr = Register(ctx, crossType);

                //correction
                FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
                ctx.CorrectIndex(2);

                //injection
                if (code == Code.Switch)
                {
                    processor.InsertBefore(instr, ldstr);
                    processor.InsertBefore(instr, call);
                    ctx.Switches.Add(instr);
                }
                else
                {
                    processor.InsertAfter(instr, call);
                    processor.InsertAfter(instr, ldstr);
                }

                processed = true;
                needBreak = true;
            }
            #endregion
            #region 'Switch when()', etc
            #region Checks
            prev = operand?.Previous;
            if (prev == null || processedInstrs.Contains(prev))
                return processed;
            prevCode = prev.OpCode.Code;
            if (prevCode is not Code.Br and not Code.Br_S and not Code.Throw)
                return processed;
            #endregion

            //data: need to insert paired call
            crossType = crossType == CrossPointType.If ? CrossPointType.Else : CrossPointType.If;
            var ind = 0;
            while (operand != null)
            {
                ind = ctx.OrigInstructions.IndexOf(operand);
                if (ind >= 0)
                    break;
                operand = operand.Previous;
            }
            ctx.Processed.Add(operand);
            var ldstr2 = Register(ctx, crossType, prevCode == Code.Throw ? ind : ind - 1); //need to sub 1 for prev index if no Throw unstruction

            //correction
            ReplaceJumps(operand, ldstr2, ctx);

            //injection
            processor.InsertBefore(operand, ldstr2);
            processor.InsertBefore(operand, call);
            if (((Instruction)instr.Operand).Offset < instr.Offset)
                ctx.CorrectIndex(2);
            
            //paired injected instructions are must be labeled as processed
            if(prevCode != Code.Throw)
                processedInstrs.Add(prev);

            needBreak = true;
            #endregion

            return true;
        }
    }
}
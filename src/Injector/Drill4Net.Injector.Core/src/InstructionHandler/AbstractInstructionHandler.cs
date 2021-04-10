using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Profiling.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractInstructionHandler
    {
        public string Name { get; set; }

        public AbstractInstructionHandler Successor { get; set; }

        protected readonly AbstractProbeHelper _probeHelper;

        /************************************************************************************/

        protected AbstractInstructionHandler(string name, AbstractProbeHelper probeHelper)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _probeHelper = probeHelper ?? throw new ArgumentNullException(nameof(probeHelper));
        }

        /************************************************************************************/

        #region Handle
        #region Starting processing the method
        public virtual void StartMethod(InjectorContext ctx)
        {
            try
            {
                StartMethodConcrete(ctx);
            }
            finally
            {
                Successor?.StartMethod(ctx);
            }
        }

        protected virtual void StartMethodConcrete(InjectorContext ctx) { }
        #endregion
        #region Handle concrete instruction
        public void HandleInstruction(InjectorContext ctx)
        {
            bool needBreak = false;
            try
            {
                HandleInstructionConcrete(ctx, out needBreak);
            }
            finally
            {
                if (!needBreak)
                    Successor?.HandleInstruction(ctx);
            }
        }

        protected abstract void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak);
        #endregion
        #endregion
        #region GetProbeData
        protected virtual string GetProbeData(InjectorContext ctx)
        {
            return _probeHelper.GetProbeData(ctx);
        }

        protected virtual string GetProbeData(InjectorContext ctx, CrossPointType pointType)
        {
            return _probeHelper.GetProbeData(ctx, pointType);
        }

        protected virtual string GetProbeData(InjectorContext ctx, CrossPointType pointType, int byIndex)
        {
            return _probeHelper.GetProbeData(ctx, pointType, byIndex);
        }
        #endregion

        internal protected bool IsRealCondition(int ind, Mono.Collections.Generic.Collection<Instruction> instructions,
            bool isAsyncStateMachine)
        {
            if (ind < 0 || ind >= instructions.Count)
                return false;
            //
            var op = instructions[ind];
            if (isAsyncStateMachine)
            {
                var prev = SkipNop(ind, false, instructions);
                var prevOpS = prev.Operand?.ToString();
                var isInternal = prev.OpCode.Code == Code.Call &&
                                    prevOpS != null &&
                                    (prevOpS.EndsWith("TaskAwaiter::get_IsCompleted()") || prevOpS.Contains("TaskAwaiter`1"))
                                    ;
                if (isInternal)
                    return false;

                //seems not good: skip some state machine's instructions

                // machine state not init jump block (yeah...)
                if (instructions[ind - 1].OpCode.Code == Code.Ldloc_0 &&
                    instructions[ind + 1].OpCode.FlowControl == FlowControl.Branch &&
                    instructions[ind + 2].OpCode.FlowControl == FlowControl.Branch &&
                    instructions[ind + 3].OpCode.Code == Code.Nop &&
                    op.Operand == instructions[ind + 2] &&
                    instructions[ind + 1].Operand == instructions[ind + 3]
                    )
                    return false;

                //1. start of finally block of state machine
                if (op.OpCode.Code == Code.Bge_S &&
                    instructions[ind - 1].OpCode.Code == Code.Ldc_I4_0 &&
                    instructions[ind - 2].OpCode.Code == Code.Ldloc_0 &&
                    instructions[ind - 3].OpCode.Code == Code.Leave_S
                    )
                    return false;

                //2. end of finally block of state machine
                if (op.OpCode.Code == Code.Brfalse_S &&
                    instructions[ind + 1].OpCode.Code == Code.Ldarg_0 &&
                    instructions[ind + 2].OpCode.Code == Code.Ldfld &&
                    instructions[ind + 3].OpCode.Code == Code.Callvirt &&
                    instructions[ind + 4].OpCode.Code == Code.Nop &&
                    instructions[ind + 5].OpCode.Code == Code.Endfinally
                    )
                    return false;
            }
            //
            var next = SkipNop(ind, true, instructions);

            return op.Operand != next; //how far do it jump?
        }

        internal protected void FixFinallyEnd(Instruction cur, Instruction on, Mono.Collections.Generic.Collection<ExceptionHandler> handlers)
        {
            var prev = cur.Previous;
            var prevCode = prev.OpCode.Code;
            if (prevCode == Code.Endfinally)
            {
                foreach (var exc in handlers)
                {
                    if (exc.HandlerEnd == cur)
                        exc.HandlerEnd = on;
                }
            }
        }

        internal protected Instruction SkipNop(int ind, bool forward, Mono.Collections.Generic.Collection<Instruction> instructions)
        {
            int start, inc;
            if (forward)
            {
                start = ind + 1;
                inc = 1;
            }
            else
            {
                start = ind - 1;
                inc = -1;
            }
            //
            for (var i = start;; i += inc)
            {
                if (i >= instructions.Count || i < 0)
                    break;
                var op = instructions[i];
                if (op.OpCode.Code == Code.Nop)
                    continue;
                return op;
            }
            return Instruction.Create(OpCodes.Nop);
        }

        internal protected void ReplaceJump(Instruction from, Instruction to, HashSet<Instruction> jumpers)
        {
            //direct jumps
            foreach (var curOp in jumpers.Where(j => j.Operand == from))
                curOp.Operand = to;

            //switches
            foreach (var curOp in jumpers.Where(j => j.OpCode.Code == Code.Switch))
            {
                var switches = (Instruction[])curOp.Operand;
                for (int i = 0; i < switches.Length; i++)
                {
                    if (switches[i] == from)
                        switches[i] = to;
                }
            }
        }

        internal protected bool IsCompilerGeneratedBranch(int ind, Mono.Collections.Generic.Collection<Instruction> instructions,
            HashSet<Instruction> compilerInstructions)
        {
            //TODO: optimize (caching 'normal instruction')
            if (ind < 0 || ind >= instructions.Count)
                return false;
            Instruction instr = instructions[ind];
            if (instr.OpCode.FlowControl != FlowControl.Cond_Branch)
                return false;
            //
            Instruction inited = instr;
            Instruction finish = inited.Operand as Instruction;
            var localInsts = new List<Instruction>();

            while (true)
            {
                if (instr == null || instr.Offset == 0)
                    break;

                //we don't need compiler generated instructions in business code
                if (!compilerInstructions.Contains(instr))
                {
                    localInsts.Add(instr);
                    var operand = instr.Operand as MemberReference;
                    if (operand?.Name.StartsWith("<") == true) //hm...
                    {
                        //add next instructions of branch as 'angled'
                        var curNext = inited.Next;
                        while (true)
                        {
                            var flow = curNext.OpCode.FlowControl;
                            if (curNext == finish || flow == FlowControl.Return || flow == FlowControl.Throw)
                                break;
                            localInsts.Add(curNext);
                            curNext = curNext.Next;
                        }

                        //add local angled instructions to cache
                        foreach (var ins in localInsts)
                            if (!compilerInstructions.Contains(ins))
                                compilerInstructions.Add(ins);
                        return true;
                    }
                }
                instr = instr.Previous;
            }
            return false;
        }

        internal protected bool IsNextReturn(int ind, Mono.Collections.Generic.Collection<Instruction> instructions, Instruction lastOp)
        {
            var ins = instructions[ind];
            if (ins.Operand is not Instruction op)
                return false;
            if (op == lastOp && ins.Next == lastOp)
                return true;
            return op.OpCode.Name.StartsWith("ldloc") && (op.Next == lastOp || op.Next?.Next == lastOp);
        }

        protected Instruction GetFirstInstruction(string probeData)
        {
            return Instruction.Create(OpCodes.Ldstr, probeData);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractBaseHandler
    {
        public string Name { get; }

        public AbstractBaseHandler Successor { get; set; }

        private readonly AbstractProbeHelper _probeHelper;

        /**************************************************************************************/

        protected AbstractBaseHandler(string name, AbstractProbeHelper probeHelper)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _probeHelper = probeHelper ?? throw new ArgumentNullException(nameof(probeHelper));
        }

        /**************************************************************************************/

        #region Handle
        #region Starting processing the method
        public virtual void StartMethod(MethodContext ctx)
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

        protected virtual void StartMethodConcrete(MethodContext ctx) { }
        #endregion
        #region Handle concrete instruction
        public void HandleInstruction(MethodContext ctx)
        {
            var needBreak = false;
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

        protected abstract void HandleInstructionConcrete(MethodContext ctx, out bool needBreak);
        #endregion
        #endregion
        #region  Register
        protected virtual Instruction Register(MethodContext ctx, CrossPointType type)
        {
            return Register(ctx, type, ctx.SourceIndex);
        }

        protected virtual Instruction Register(MethodContext ctx, CrossPointType type, int ind, bool asProcessed = true)
        {
            if (ind < 0)
                throw new ArgumentException(nameof(ind));
            if (asProcessed)
                ctx.AheadProcessed.Add(ctx.Instructions[ind]);
            var probeData = GetProbeData(ctx, type, ind, out var point);
            return GetFirstInstruction(ctx, probeData);
        }

        protected virtual string GetProbeData(MethodContext ctx, CrossPointType pointType, int byIndex, out CrossPoint point)
        {
            point = GetPoint(ctx, pointType, byIndex);
            return _probeHelper.GenerateProbe(ctx, point);
        }
        
        protected virtual CrossPoint GetPoint(MethodContext ctx, CrossPointType pointType, int byIndex)
        {
            return _probeHelper.GetPoint(ctx, pointType, byIndex);
        }

        protected virtual string GetProbeData(MethodContext ctx, CrossPointType pointType, int byIndex)
        {
            return GetProbeData(ctx, pointType, byIndex, out var _);
        }
        #endregion

        internal bool IsRealCondition(int ind, Mono.Collections.Generic.Collection<Instruction> instructions)
        {
            if (ind < 0 || ind >= instructions.Count)
                return false;
            //
            var op = instructions[ind];
            var next = SkipNop(ind, true, instructions);
            return op.Operand != next; //how far do it jump?
        }

        internal void FixFinallyEnd(Instruction cur, Instruction on, IEnumerable<ExceptionHandler> handlers)
        {
            if (cur.Previous == null)
                return;
            if (cur.Previous.OpCode.Code != Code.Endfinally) 
                return;
            foreach (var exc in handlers.Where(exc => exc.HandlerEnd == cur))
            {
                exc.HandlerEnd = on;
            }
        }

        internal Instruction SkipNop(int ind, bool forward, Mono.Collections.Generic.Collection<Instruction> instructions)
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

        internal void ReplaceJumps(Instruction from, Instruction to, MethodContext ctx)
        {
            ctx.ReplacedJumps.Add(to, from);
            var jumpers = ctx.Jumpers;
            
            //direct jumps
            foreach (var curOp in jumpers.Where(j => j.Operand == from))
                curOp.Operand = to;

            //switches
            foreach (var curOp in jumpers.Where(j => j.OpCode.Code == Code.Switch))
            {
                var switches = (Instruction[])curOp.Operand;
                for (var i = 0; i < switches.Length; i++)
                {
                    if (switches[i] == from)
                        switches[i] = to;
                }
            }
        }

        internal bool IsCompilerGeneratedBranch(int ind, Mono.Collections.Generic.Collection<Instruction> instructions,
            HashSet<Instruction> compilerInstructions)
        {
            //TODO: optimize (caching 'normal instruction')
            if (ind < 0 || ind >= instructions.Count)
                return false;
            var instr = instructions[ind];
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
                            if (curNext == finish || flow is FlowControl.Return or FlowControl.Throw)
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

        internal bool IsNextReturn(int ind, Mono.Collections.Generic.Collection<Instruction> instructions, Instruction lastOp)
        {
            var ins = instructions[ind];
            if (ins.Operand is not Instruction op)
                return false;
            if (op == lastOp && ins.Next == lastOp)
                return true;
            return op.OpCode.Name.StartsWith("ldloc") && (op.Next == lastOp || op.Next?.Next == lastOp);
        }

        protected Instruction GetFirstInstruction(MethodContext ctx, string probeData)
        {
            var instr = Instruction.Create(OpCodes.Ldstr, probeData);
            ctx.FirstInjectInstructions.Add(instr);
            return instr;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

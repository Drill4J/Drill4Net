using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Base Abstract Handler for IL instruction
    /// </summary>
    public abstract class AbstractBaseHandler
    {
        /// <summary>
        /// Name of Instruction Handler
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Next instruction handler from the chain of such hanlers
        /// </summary>
        public AbstractBaseHandler Successor { get; set; }

        protected readonly AbstractProbeHelper _probeHelper;

        /**************************************************************************************/

        /// <summary>
        /// Base Abstract Handler of IL instruction
        /// </summary>
        /// <param name="name">Handler's name</param>
        /// <param name="probeHelper">Helper for </param>
        protected AbstractBaseHandler(string name, AbstractProbeHelper probeHelper)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _probeHelper = probeHelper ?? throw new ArgumentNullException(nameof(probeHelper));
        }

        /**************************************************************************************/

        #region Handle
        #region Preprocess the method
        /// <summary>
        /// Preprocess method context
        /// </summary>
        /// <param name="ctx">Method's context</param>
        public virtual void Preprocess(MethodContext ctx)
        {
            try
            {
                PreprocessConcrete(ctx);
            }
            finally
            {
                Successor?.Preprocess(ctx);
            }
        }

        /// <summary>
        /// Process method contex. Can be overridden in the descendant class.
        /// </summary>
        /// <param name="ctx">Method's context</param>
        protected virtual void PreprocessConcrete(MethodContext ctx) { }
        #endregion
        #region Handle concrete instruction
        /// <summary>
        /// Handle concrete instruction
        /// </summary>
        /// <param name="ctx">Method's context</param>
        public void HandleInstruction(MethodContext ctx)
        {
            var needBreak = false;
            try
            {
                if (HandleInstructionConcrete(ctx, out needBreak))
                    ctx.RegisterProcessed();
            }
            finally
            {
                if (!needBreak)
                    Successor?.HandleInstruction(ctx);
            }
        }

        /// <summary>
        /// Handle concrete instruction. Must be overridden in the descendant class.
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="needBreak">If the current instruction was processed either need to break further processing by next handlers?</param>
        protected abstract bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak);
        #endregion
        #region Postprocess the method
        /// <summary>
        /// Postprocess method context
        /// </summary>
        /// <param name="ctx">Method's context</param>
        public virtual void Postprocess(MethodContext ctx)
        {
            try
            {
                PostprocessConcrete(ctx);
            }
            finally
            {
                Successor?.Postprocess(ctx);
            }
        }

        /// <summary>
        /// Postprocess method contex. Can be overridden in the descendant class.
        /// </summary>
        /// <param name="ctx">Method's context</param>
        protected virtual void PostprocessConcrete(MethodContext ctx) { }
        #endregion
        #endregion
        #region  Register
        /// <summary>
        /// Register the cross-point by its type and current instruction's index, and get the instruction which need to inject
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="type">Type of the cross-point</param>
        /// <returns></returns>
        protected virtual Instruction Register(MethodContext ctx, CrossPointType type)
        {
            return Register(ctx, type, ctx.OrigIndex);
        }

        /// <summary>
        /// Register the cross-point by its type and certain instruction's index, and get the instruction which need to inject
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="type">Type of the cross-point</param>
        /// <param name="origInd">Certain (not current) index of instruction for the cross-point</param>
        /// <param name="asProcessed">Is the instruction considered processed?</param>
        /// <returns></returns>
        protected virtual Instruction Register(MethodContext ctx, CrossPointType type, int origInd, bool asProcessed = true)
        {
            if (origInd < 0)
                throw new ArgumentException(nameof(origInd));
            if (asProcessed)
                ctx.Processed.Add(ctx.OrigInstructions[origInd]);
            var probeData = GetProbeData(ctx, type, origInd, out var point);
            return GetStartingInjectedInstruction(ctx, probeData);
        }

        /// <summary>
        /// Get string data of probe for the injecting
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="pointType">Type of the cross-point</param>
        /// <param name="origIndex">Index of instruction as local ID of cross-point</param>
        /// <param name="point">Object of the cross-point</param>
        /// <returns></returns>
        protected virtual string GetProbeData(MethodContext ctx, CrossPointType pointType, int origIndex, out CrossPoint point)
        {
            point = GetPoint(ctx, pointType, origIndex);
            return _probeHelper.GenerateProbe(ctx, point);
        }

        /// <summary>
        /// Object of the cross-point for the injecting
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="pointType">Type of the cross-point</param>
        /// <param name="origIndex">Index of instruction as local ID of cross-point</param>
        /// <returns></returns>
        protected virtual CrossPoint GetPoint(MethodContext ctx, CrossPointType pointType, int origIndex)
        {
            return _probeHelper.GetOrCreatePoint(ctx, pointType, origIndex);
        }

        /// <summary>
        /// Get string data of probe for the injecting
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="pointType">Type of the cross-point</param>
        /// <param name="origIndex">Index of instruction as local ID of cross-point</param>
        /// <returns></returns>
        protected virtual string GetProbeData(MethodContext ctx, CrossPointType pointType, int origIndex)
        {
            return GetProbeData(ctx, pointType, origIndex, out var _);
        }
        #endregion

        /// <summary>
        /// The current branch is the real condition?
        /// </summary>
        /// <param name="ind">Current index of instruction</param>
        /// <param name="instructions">List of method's instructions</param>
        /// <returns></returns>
        protected bool IsRealCondition(int ind, MethodContext ctx)
        {
            var instructions = ctx.Instructions;
            if (ind < 0 || ind >= instructions.Count)
                return false;
            //
            var op = instructions[ind];
            var next = SkipNop(ind, true, ctx);
            return op.Operand != next; //how far do it jump?
        }

        /// <summary>
        /// Change the target for the ExceptionHandler (Try/Catch/Finally statement)
        /// </summary>
        /// <param name="cur">Current instruction (original) earler connecting to HandlerEnd</param>
        /// <param name="to">Instruction for repacing the HandlerEnd</param>
        /// <param name="handlers">List of try/cacth handlers of current method</param>
        protected void FixFinallyEnd(Instruction cur, Instruction to, IEnumerable<ExceptionHandler> handlers)
        {
            if (cur.Previous == null)
                return;

            //guanito: check it!!!
            var prevCode = cur.Previous.OpCode.Code;
            if (prevCode != Code.Endfinally && prevCode != Code.Throw)
                return;

            foreach (var exc in handlers.Where(exc => exc.HandlerEnd == cur))
            {
                exc.HandlerEnd = to;
            }
        }

        /// <summary>
        /// Skip the NOP instruction
        /// </summary>
        /// <param name="ind">Current index</param>
        /// <param name="forward">Direction: forward or backward</param>
        /// <param name="instructions">List of method's instrunctions</param>
        /// <returns></returns>
        protected Instruction SkipNop(int ind, bool forward, MethodContext methodCtx)
        {
            var instructions = methodCtx.Instructions;
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
                if (op.OpCode.Code == Code.Nop && !methodCtx.Anchors.Contains(op))
                    continue;
                return op;
            }
            return Instruction.Create(OpCodes.Nop);
        }

        /// <summary>
        /// Replace instruction "from" as target for the method's jumpers to the "to" instruction
        /// </summary>
        /// <param name="from">Original jump-target instruction</param>
        /// <param name="to">Final jump-target instruction</param>
        /// <param name="ctx">Method context</param>
        protected void ReplaceJumps(Instruction from, Instruction to, MethodContext ctx)
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

        /// <summary>
        /// The checking whether we are located in the branch generated by compiler
        /// </summary>
        /// <param name="ind">Current index of instructions</param>
        /// <param name="instructions">List of method's instructions</param>
        /// <param name="compilerInstructions">Hashed list of the compiler branch's instructions</param>
        /// <returns></returns>
        internal bool IsCompilerGeneratedBranch(int ind, Mono.Collections.Generic.Collection<Instruction> instructions,
            HashSet<Instruction> compilerInstructions)
        {
            //TODO: Check: is there any point in such a check function?

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

            //GUANO!!!
            while (instr != null && instr.Offset != 0)
            {
                //we don't need compiler generated instructions in business code
                if (!compilerInstructions.Contains(instr))
                {
                    localInsts.Add(instr);
                    var operand = instr.Operand as MemberReference;
                    if (operand?.Name.StartsWith("<") == true) //hm...
                    {
                        //add next instructions of branch as 'from compiler'
                        var curNext = inited.Next;
                        while (true)
                        {
                            var flow = curNext.OpCode.FlowControl;
                            if (curNext == finish || flow is FlowControl.Return or FlowControl.Throw)
                                break;
                            localInsts.Add(curNext);
                            curNext = curNext.Next;
                        }

                        //add local 'from compiler' instructions to cache
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

        /// <summary>
        /// This statement leads to the return statement?
        /// </summary>
        /// <param name="ind">Index of current instruction</param>
        /// <param name="instructions">List of instructions</param>
        /// <param name="lastOp">Real last operation (Return statement)</param>
        /// <returns></returns>
        internal bool IsNextReturn(int ind, MethodContext ctx, Instruction lastOp)
        {
            var ins = SkipNop(ind, true, ctx);
            if (ins.Operand is not Instruction op)
                return false;
            if (op == lastOp && ins.Next == lastOp)
                return true;
            return op.OpCode.Name.StartsWith("ldloc") && (op.Next == lastOp || op.Next?.Next == lastOp);
        }

        /// <summary>
        /// Get the set of the starting injected instructions in the injecting block. Now it's just type is OpCodes.Ldstr
        /// </summary>
        /// <param name="ctx">Method context</param>
        /// <param name="probeData">String which need injected</param>
        /// <returns></returns>
        protected Instruction GetStartingInjectedInstruction(MethodContext ctx, string probeData)
        {
            var instr = Instruction.Create(OpCodes.Ldstr, probeData);
            ctx.StartingInjectInstructions.Add(instr);
            ctx.LastAddedStartingInjectInstructions = instr;
            return instr;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

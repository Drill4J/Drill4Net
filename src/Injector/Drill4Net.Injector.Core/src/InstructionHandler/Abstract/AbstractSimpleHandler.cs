﻿using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Abstract handler for the IL code's instructions in some simple cases
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractBaseHandler" />
    public abstract class AbstractSimpleHandler : AbstractBaseHandler
    {
        /// <summary>
        /// Gets the type of the cross-point.
        /// </summary>
        /// <value>
        /// The type of the point.
        /// </value>
        public CrossPointType PointType { get; }

        /// <summary>
        /// Whether it is necessary to move the anchor for the jump from the 
        /// original instruction to the beginning of the injection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [replace jumps]; otherwise, <c>false</c>.
        /// </value>
        public bool IsReplaceJumps { get; }
        
        /*****************************************************************************/
        
        protected AbstractSimpleHandler(string name, CrossPointType pointType, AbstractProbeHelper probeHelper, bool replaceJump = true):
            base(name, probeHelper)
        {
            IsReplaceJumps = replaceJump;
            PointType = pointType;
        }

        /*****************************************************************************/

        /// <summary>
        /// Instruction handler for the simple cases by current method's context
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="needBreak">Do we need to break the further processing in the chain of 
        /// subsequent handlers after successful processing by the current handler?</param>
        protected override bool HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            
            if (!IsCondition(ctx))
                return false;

            ProcessInstruction(ctx);
            PostAction(ctx);

            needBreak = true;
            return true;
        }

        protected virtual void ProcessInstruction(MethodContext ctx)
        {
            var instr = ctx.CurInstruction;
            var ldstr = Register(ctx, PointType);
            var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

            //correction
            FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
            if(IsReplaceJumps)
                ReplaceJumps(instr, ldstr, ctx);
            ctx.CorrectIndex(2);

            //injection
            var processor = ctx.Processor;
            processor.InsertBefore(instr, ldstr);
            processor.InsertBefore(instr, call);
        }

        /// <summary>
        /// Check whether the current instruction needs to be processed by this handler
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected abstract bool IsCondition(MethodContext ctx);

        /// <summary>
        /// Post action after main injection for the current instruction.
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void PostAction(MethodContext ctx) {}
    }
}
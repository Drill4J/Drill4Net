using Mono.Cecil.Cil;
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
        
        /*****************************************************************************/
        
        private protected AbstractSimpleHandler(string name, CrossPointType pointType, AbstractProbeHelper probeHelper):
            base(name, probeHelper)
        {
            PointType = pointType;
        }

        /*****************************************************************************/

        /// <summary>
        /// Instruction handler for the simple cases by current method's context
        /// </summary>
        /// <param name="ctx">Method's context</param>
        /// <param name="needBreak">Do we need to break the further processing in the chain of 
        /// subsequent handlers after successful processing by the current handler?</param>
        protected override void HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            
            if (!IsCondition(ctx))
                return;

            //data
            var instr = ctx.Instructions[ctx.CurIndex];
            var ldstr = Register(ctx, PointType);
            var call = Instruction.Create(OpCodes.Call, ctx.AssemblyCtx.ProxyMethRef);

            //correction
            FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
            ReplaceJumps(instr, ldstr, ctx);
            ctx.CorrectIndex(2);

            //injection
            var processor = ctx.Processor;
            processor.InsertBefore(instr, ldstr);
            processor.InsertBefore(instr, call);

            PostAction(ctx);   
            needBreak = true;
        }

        /// <summary>
        /// Check whether the current instruction needs to be processed by this handler
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected abstract bool IsCondition(MethodContext ctx);

        /// <summary>
        /// Post action after main injection.
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void PostAction(MethodContext ctx) {}
    }
}
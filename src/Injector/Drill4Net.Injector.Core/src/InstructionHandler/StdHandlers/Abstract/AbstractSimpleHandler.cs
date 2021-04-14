using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractSimpleHandler : AbstractBaseHandler
    {
        public CrossPointType PointType { get; }
        
        /*****************************************************************************/
        
        private protected AbstractSimpleHandler(string name, CrossPointType pointType, AbstractProbeHelper probeHelper) : 
            base(name, probeHelper)
        {
            PointType = pointType;
        }

        /*****************************************************************************/

        protected override void HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            needBreak = false;
            
            if (!IsCondition(ctx))
                return;
            
            //data
            var probData = GetProbeData(ctx);
            var ldstr = GetFirstInstruction(ctx, probData);
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            
            //correction
            var instr = ctx.Instructions[ctx.CurIndex];
            FixFinallyEnd(instr, ldstr, ctx.ExceptionHandlers); //need fix statement boundaries for potential try/finally 
            ReplaceJumps(instr, ldstr, ctx);
            
            //injection
            var processor = ctx.Processor;
            processor.InsertBefore(instr, ldstr);
            processor.InsertBefore(instr, call);
            ctx.IncrementIndex(2);

            PostAction(ctx);
            
            needBreak = true;
        }

        protected abstract bool IsCondition(MethodContext ctx);
        
        protected virtual string GetProbeData(MethodContext ctx)
        {
            return _probeHelper.GetProbeData(ctx, PointType);
        }

        protected virtual void PostAction(MethodContext ctx) {}
    }
}
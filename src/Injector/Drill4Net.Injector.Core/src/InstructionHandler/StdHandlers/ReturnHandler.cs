using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ReturnHandler : AbstractBaseHandler
    {
        protected Instruction _returnInst; //must be persistent

        /**************************************************************************************/

        public ReturnHandler(AbstractProbeHelper probeHelper) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_RETURN, probeHelper)
        {          
        }

        /**************************************************************************************/

        protected override void StartMethodConcrete(MethodContext ctx)
        {
            //data
            _returnInst = Register(ctx, CrossPointType.Return, -1); //as object it must be only one and localId AT THIS MOMENT is exactly -1 !
        }

        protected override void HandleInstructionConcrete(MethodContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var strictEnterReturn = ctx.IsStrictEnterReturn;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            var lastOp = ctx.LastOperation;
            #endregion

            if (strictEnterReturn)
                return;

            var point = ctx.Method.Filter(typeof(CrossPoint), false)
                .Cast<CrossPoint>()
                .FirstOrDefault(a => a.PointType == CrossPointType.Return);
            if (point != null)
            {
                //the return in the middle of the method body
                if (instr.Operand == lastOp && lastOp.OpCode.Code != Code.Endfinally) //jump to the end for return from function
                {
                    _returnInst.Operand = Register(ctx, CrossPointType.Return, point.BusinessIndex); // $"{_initProbData}{_probeHelper.GenerateProbeData(point)}";
                    instr.Operand = _returnInst;
                }

                //RETURN
                if (code != Code.Ret)
                    return;

                //correcting pointId
                if (point.BusinessIndex == -1)
                {
                    point.PointId = ctx.CurIndex.ToString();
                    point.BusinessIndex = ctx.SourceIndex; //it's properly only for the business method (but Enter/Return not injected into CG methods...)
                }

                //data
                _returnInst.Operand = GetProbeData(ctx, CrossPointType.Return, point.BusinessIndex); // $"{_initProbData}{_probeHelper.GenerateProbeData(point)}";
                //Register(ctx, point);
                
                //correction
                FixFinallyEnd(instr, _returnInst, exceptionHandlers);
                ReplaceJumps(instr, _returnInst, ctx);

                //injection
                processor.InsertBefore(instr, _returnInst);
                processor.InsertBefore(instr, call);
                ctx.IncrementIndex(2);
            }

            needBreak = true;
        }
    }
}

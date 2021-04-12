using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    public class CallHandler : AbstractSimpleHandler
    {
        protected readonly TypeChecker _typeChecker;
        
        /*****************************************************************************/
        
        public CallHandler(AbstractProbeHelper probeHelper): 
            base(C.INSTRUCTION_HANDLER_CALL, CrossPointType.Call, probeHelper)
        {
            _typeChecker = new TypeChecker();
        }
        
        /*****************************************************************************/

        protected override bool IsCondition(InjectorContext ctx)
        {
            try
            {
                var instr = ctx.Instructions[ctx.CurIndex];
                //1. Code.Callvirt isn't needed - it's for internal compiler generated member's calls
                //2. Local function's calls are needed
                if (instr.OpCode.Code is not Code.Call and not Code.Calli)
                    return false;
                var operand = (MethodReference) instr.Operand;
                if (operand.FullName.Contains(ctx.ProxyNamespace))
                    return false;
                var res = _typeChecker.CheckByMethodName(operand.FullName);
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
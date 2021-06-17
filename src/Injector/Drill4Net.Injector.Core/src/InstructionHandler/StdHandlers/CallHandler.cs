using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler of the cross-point for the "Call" type (call another business-method from current one)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractSimpleHandler" />
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

        protected override bool IsCondition(MethodContext ctx)
        {
            try
            {
                var instr = ctx.Instructions[ctx.CurIndex];
                var code = instr.OpCode.Code;
                //1. Code.Callvirt isn't needed - it's for internal compiler generated member's calls
                //2. Local function's calls are needed
                if (code is not Code.Call and not Code.Calli and not Code.Callvirt)
                    return false;
                var operand = (MethodReference) instr.Operand;
                var fullname = operand.FullName;
                if (fullname.EndsWith("AsyncTaskMethodBuilder::Create()"))
                    return true; //because it starts the Async State Machine with its own points
                if (fullname.Contains("get__") || fullname.Contains("set__")) //ASP.NET/Blazor methods - not needed
                    return false;
                var isOwn = _typeChecker.CheckByMethodFullName(fullname);
                if (code is Code.Callvirt)
                    return isOwn || fullname.Contains("::Invoke(");
                if (fullname.Contains(ctx.TypeCtx.AssemblyCtx.ProxyNamespace))
                    return false;
                return isOwn;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
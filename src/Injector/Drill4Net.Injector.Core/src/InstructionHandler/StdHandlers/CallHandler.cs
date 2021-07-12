using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using static Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Call" type (call another business-method from current one)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractSimpleHandler" />
    public class CallHandler : AbstractSimpleHandler
    {
        protected readonly TypeChecker _typeChecker;
        
        /*****************************************************************************/
        
        public CallHandler(AbstractProbeHelper probeHelper):
            base(INSTRUCTION_HANDLER_CALL, CrossPointType.Call, probeHelper)
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
                var operand = (MethodReference)instr.Operand;
                var callFullname = operand.FullName;
                if (callFullname.Contains(ctx.TypeCtx.AssemblyCtx.ProxyNamespace)) //own injection
                    return false;
                if (callFullname.EndsWith("AsyncTaskMethodBuilder::Start()")) //Create
                    return true; //because it starts the Async State Machine with its own points
                if (callFullname.Contains("get__") || callFullname.Contains("set__")) //ASP.NET/Blazor methods - not needed
                    return false;
                var invoke = code is Code.Callvirt && callFullname.Contains("::Invoke(");
                if (invoke)
                    return true;

                var isAnon = _typeChecker.IsAnonymousType(callFullname);
                if (isAnon)
                    return false;

                // check by filter
                var ns = CommonUtils.GetNamespace(CommonUtils.GetTypeByMethod(callFullname));
                if (ns == null)
                    return true; //hmmm...
                var flt = _probeHelper.Options.Source.Filter;
                var nsNeeded = flt.IsNamespaceNeed(ns);
                return nsNeeded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
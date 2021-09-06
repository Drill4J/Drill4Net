using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using static Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// IL code's handler for the cross-point of the "Call" type (call another business-method from current one)
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractSimpleHandler" />
    public class CallHandler : AbstractSimpleHandler
    {
        protected readonly TypeChecker _typeChecker;
        private readonly Logger _logger;

        /*****************************************************************************/

        public CallHandler(AbstractProbeHelper probeHelper):
            base(INSTRUCTION_HANDLER_CALL, CrossPointType.Call, probeHelper)
        {
            _logger = new TypedLogger<CallHandler>(CoreConstants.SUBSYSTEM_INJECTOR);
            _typeChecker = new TypeChecker();
        }
        
        /*****************************************************************************/

        protected override bool IsCondition(MethodContext ctx)
        {
            try
            {
                var curInd = ctx.CurIndex;
                var instrs = ctx.Instructions;
                var instr = instrs[curInd];
                var code = instr.OpCode.Code;
                //1. Code.Callvirt isn't needed - it's for internal compiler generated member's calls
                //2. Local function's calls are needed
                if (code is not Code.Call and not Code.Calli and not Code.Callvirt)
                    return false;
                var callees = ctx.Method.CalleeOrigIndexes;
                var operand = (MethodReference)instr.Operand;
                var operType = operand.DeclaringType;
                var callFullname = operand.FullName;
                if (callFullname.Contains(ctx.TypeCtx.AssemblyCtx.ProxyNamespace)) //own injection
                    return false;
                if (callFullname.Contains(".AsyncTaskMethodBuilder") && callFullname.Contains("::Start"))
                    return true; //because it starts the Async State Machine with its own points
                if (callFullname.Contains(".Task::Run(") || callFullname.Contains(".Task::Run<"))
                    return true;
                if (callFullname.Contains("Tasks.TaskFactory::Start"))
                    return true;
                if (callFullname.Contains(".Tasks.Parallel::"))
                    return true;
                if (operType.FullName == "System.Linq.Enumerable" || operType.FullName.StartsWith("System.Linq.Parallel"))
                    return true; //because can be user logic in LINQ expression
                if (code is Code.Callvirt && callFullname.EndsWith("Threading.Thread::Start()"))
                    return true;
                if (callFullname.Contains("get__") || callFullname.Contains("set__")) //ASP.NET/Blazor methods - not needed
                    return false;
                var invoke = code is Code.Callvirt && callFullname.Contains("::Invoke(");
                if (invoke)
                    return true;

                var isAnon = _typeChecker.IsAnonymousType(callFullname);
                if (isAnon)
                    return false;

                //using linked CG methods in various call (DI frameworks, AutoMapper, Mediatr, LINQ, etc)
                var aboveUsingCg = AboveUsedLinkedCgMethod(instrs, curInd);
                if (aboveUsingCg)
                    return true;
                var nearestStsfldInd = FindNearestStsfld(instrs, curInd);
                if(nearestStsfldInd != -1)
                    aboveUsingCg = AboveUsedLinkedCgMethod(instrs, nearestStsfldInd+1);
                if (aboveUsingCg)
                    return true;

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
                _logger.Error("Error for defining condition for instruction", e);
                throw;
            }
        }

        private bool AboveUsedLinkedCgMethod(Mono.Collections.Generic.Collection<Instruction> instrs, int curInd)
        {
            if (curInd > 2 && instrs[curInd - 1].OpCode.Code == Code.Newobj && instrs[curInd - 2].OpCode.Code == Code.Ldftn)
                return true;
            if (curInd > 3 && instrs[curInd - 1].OpCode.Code == Code.Stsfld && instrs[curInd - 2].OpCode.Code == Code.Dup && instrs[curInd - 3].OpCode.Code == Code.Newobj)
                return true;
            return false;
        }

        private int FindNearestStsfld(Mono.Collections.Generic.Collection<Instruction> instrs, int curInd)
        {
            var to = curInd - 10;
            if (to < 0)
                to = 0;
            for (var i = curInd; i >= to; i--)
            {
                if (instrs[i].OpCode.Code == Code.Stsfld)
                    return i;
            }
            return -1;
        }
    }
}
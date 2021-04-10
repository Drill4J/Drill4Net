using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowStrategy : InstructionHandlerStrategy
    {
        public FlowStrategy()
        {
            var helper = new FlowProbeHelper();
            
            ConnectHandler(new EnterHandler(helper));
            ConnectHandler(new IfElseHandler(helper));
            ConnectHandler(new CycleHandler(helper));
            ConnectHandler(new NonConditionBranchHandler(helper));
            ConnectHandler(new CatchFilterHandler(helper));
            ConnectHandler(new ThrowHandler(helper));
            ConnectHandler(new ReturnHandler(helper));
        }
    }
}

using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowStrategy : InstructionHandlerStrategy
    {
        public FlowStrategy()
        {
            ConnectHandler(new EnterHandler());
            ConnectHandler(new ConditionBranchHandler());
            ConnectHandler(new NonConditionBranchHandler());
            ConnectHandler(new CatchFilterHandler());
            ConnectHandler(new ThrowHandler());
            ConnectHandler(new ReturnHandler());
        }
    }
}

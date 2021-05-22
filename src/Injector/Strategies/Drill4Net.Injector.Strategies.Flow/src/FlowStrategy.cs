using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowStrategy : CodeHandlerStrategy
    {
        public FlowStrategy(ProbesOptions opts = null)
        {
            var helper = new FlowProbeHelper();
            
            //branches
            ConnectHandler(new IfElseHandler(helper));
            ConnectHandler(new NonConditionBranchHandler(helper));
            ConnectHandler(new CycleHandler(helper));
            
            //catch, throw
            ConnectHandler(new ThrowHandler(helper));
            ConnectHandler(new CatchFilterHandler(helper));
            
            //enter/return
            ConnectHandler(new ReturnHandler(helper));
            if(opts == null || !opts.SkipEnterType)
                ConnectHandler(new EnterHandler(helper));
            
            //methods' calls (must be prior AnchorHandler)
            ConnectHandler(new CallHandler(helper));

            //jump targets + call of compiler generated members
            ConnectHandler(new AnchorHandler(helper, true)); //TODO: automatic check for CycleHandler's existing?
        }
    }
}

﻿using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowStrategy : InstructionHandlerStrategy
    {
        public FlowStrategy()
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
            ConnectHandler(new EnterHandler(helper));
            ConnectHandler(new ReturnHandler(helper));
            
            //methods' calls
            ConnectHandler(new CallHandler(helper));
            
            //jump targets
            ConnectHandler(new AnchorHandler(helper));
        }
    }
}

using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowStrategy : CodeHandlerStrategy
    {
        public FlowStrategy(ProbesOptions probeOpts = null, InjectorDebugOptions dbgOpts = null)
        {
            var helper = new FlowProbeHelper(dbgOpts);
            
            //branches
            AddHandler(new IfElseHandler(helper));
            AddHandler(new NonConditionBranchHandler(helper));
            AddHandler(new CycleHandler(helper));
            
            //catch, throw
            AddHandler(new ThrowHandler(helper));
            AddHandler(new CatchFilterHandler(helper));
            
            //enter/return
            AddHandler(new ReturnHandler(helper));
            if(probeOpts == null || !probeOpts.SkipEnterType)
                AddHandler(new EnterHandler(helper));
            
            //methods' calls (must be prior AnchorHandler)
            AddHandler(new CallHandler(helper));

            //jump targets + call of compiler generated members
            AddHandler(new AnchorHandler(helper, true)); //TODO: automatic check for CycleHandler's existing?
        }
    }
}

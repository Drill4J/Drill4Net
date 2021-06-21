using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Flow
{
    /// <summary>
    /// Strategy for target's injection with classics Flow cross-points
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.CodeHandlerStrategy" />
    public class FlowStrategy : CodeHandlerStrategy
    {
        public FlowStrategy(ProbesOptions probeOpts = null, InjectorDebugOptions dbgOpts = null)
        {
            var helper = new FlowProbeHelper(dbgOpts);
            
            //branches
            AddHandler(new IfElseHandler(helper));
            AddHandler(new NonConditionBranchHandler(helper));
            AddHandler(new CycleHandler(helper));

            AddHandler(new JumperHandler(helper));

            //catch, throw
            AddHandler(new ThrowHandler(helper));
            AddHandler(new CatchFilterHandler(helper));
            
            //enter/return
            AddHandler(new ReturnHandler(helper));
            if(probeOpts?.SkipEnterType != true)
                AddHandler(new EnterHandler(helper));
            
            //methods' calls (must be prior AnchorHandler)
            AddHandler(new CallHandler(helper));

            //jump targets + call of compiler generated members
            AddHandler(new AnchorHandler(helper, true)); //TODO: automatic check for CycleHandler's existing?
        }
    }
}

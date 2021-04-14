using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockNonConditionBranchHandler : NonConditionBranchHandler
    {
        public BlockNonConditionBranchHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }
        
        /********************************************************************************************/
        
        protected virtual string GetProbeData(MethodContext ctx)
        {
            return _probeHelper.GetProbeData(ctx);
        }
    }
}
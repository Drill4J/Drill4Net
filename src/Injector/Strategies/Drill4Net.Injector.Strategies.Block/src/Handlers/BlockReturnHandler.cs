using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockReturnHandler : ReturnHandler
    {
        public BlockReturnHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }
        
        /*******************************************************************************/
        
        protected override string GetProbeData(InjectorContext ctx)
        {
            return _probeHelper.GetProbeData(ctx);
        }
    }
}
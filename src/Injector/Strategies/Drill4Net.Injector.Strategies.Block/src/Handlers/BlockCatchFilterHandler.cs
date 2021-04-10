using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockCatchFilterHandler : CatchFilterHandler
    {
        public BlockCatchFilterHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }
        
        /*******************************************************************************/
        
        protected override string GetProbeData(InjectorContext ctx)
        {
            return _probeHelper.GetProbeData(ctx);
        }
    }
}
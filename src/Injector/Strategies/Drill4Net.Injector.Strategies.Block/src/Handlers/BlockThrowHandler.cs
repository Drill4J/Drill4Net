using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockThrowHandler : ThrowHandler
    {
        public BlockThrowHandler(AbstractProbeHelper probeHelper) : base(probeHelper)
        {
        }
        
        /*******************************************************************************/
        
        protected override string GetProbeData(InjectorContext ctx)
        {
            return _probeHelper.GetProbeData(ctx);
        }
    }
}
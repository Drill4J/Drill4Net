using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockProbeHelper : AbstractProbeHelper
    {
        protected override string GenerateProbeData(MethodContext ctx, CrossPoint point)
        {
            var injMeth = ctx.Method;
            return $"{point.PointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^{point.PointType}_{point.PointId}";
        }
    }
}
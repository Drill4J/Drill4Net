using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowProbeHelper : AbstractProbeHelper
    {
        protected override string GenerateProbePrefix(MethodContext ctx, CrossPoint point)
        {
            var injMeth = ctx.Method;
            return $"{point.PointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^";
        }

        public override string GenerateProbeData(CrossPoint point)
        {
            var str = point.PointType;
            return point.PointId == point.BusinessIndex.ToString()
                ? $"{str}_{point.BusinessIndex}"
                : $"{str}_{point.PointId}/{point.BusinessIndex}";
        }
    }
}
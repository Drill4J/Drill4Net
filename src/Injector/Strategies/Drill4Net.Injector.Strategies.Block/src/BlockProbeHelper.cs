using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockProbeHelper : AbstractProbeHelper
    {
        protected override string GenerateProbePrefix(MethodContext ctx, CrossPoint point)
        {
            var injMeth = ctx.Method;
            return $"{point.PointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^";
        }

        public override string GenerateProbeData(MethodContext ctx, CrossPoint point)
        {
            var str = point.PointType;
            return point.PointId == point.BusinessIndex.ToString()
                ? $"{str}_{point.BusinessIndex}"
                : $"{str}_{point.PointId}/{point.BusinessIndex}";
        }
    }
}
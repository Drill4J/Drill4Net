using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockProbeHelper : AbstractProbeHelper
    {
        #region GetProbeData
        public override string GetProbeData(InjectorContext ctx)
        {
            return GetProbeData(ctx, CrossPointType.Anchor, ctx.CurIndex);
        }
        
        public override string GetProbeData(InjectorContext ctx, CrossPointType pointType)
        {
            return GetProbeData(ctx, pointType, ctx.CurIndex);
        }

        public override string GetProbeData(InjectorContext ctx, CrossPointType pointType, int byIndex)
        {
            var point = CreateCrossPoint(ctx, CrossPointType.Anchor, byIndex); //yet only one Block type 
            return GenerateProbeData(ctx, point);
        }
        #endregion
        
        protected override string GenerateProbeData(InjectorContext ctx, CrossPoint point)
        {
            var injMeth = ctx.TreeMethod;
            return $"{point.PointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^{point.PointType}_{point.PointId}";
        }
    }
}
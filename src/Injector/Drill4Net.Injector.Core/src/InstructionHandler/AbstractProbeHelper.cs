using System;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractProbeHelper
    {
        #region GetProbeData
        public virtual string GetProbeData(InjectorContext ctx)
        {
            return GetProbeData(ctx, CrossPointType.Unset, ctx.CurIndex);
        }
        
        public virtual string GetProbeData(InjectorContext ctx, CrossPointType pointType)
        {
            return GetProbeData(ctx, pointType, ctx.CurIndex);
        }

        public virtual string GetProbeData(InjectorContext ctx, CrossPointType pointType, int byIndex)
        {
            var point = CreateCrossPoint(ctx, pointType, byIndex);
            return GenerateProbeData(ctx, point);
        }
        #endregion

        protected abstract string GenerateProbeData(InjectorContext ctx, CrossPoint point);

        protected virtual CrossPoint CreateCrossPoint(InjectorContext ctx, CrossPointType pointType, int localId)
        {
            var id = localId == -1 ? null : localId.ToString();
            var pointUid = Guid.NewGuid().ToString();

            var point = new CrossPoint(pointUid, id, pointType)
            {
                //TODO: PDB data
            };
            ctx.TreeMethod.AddChild(point);

            return point;
        }
    }
}

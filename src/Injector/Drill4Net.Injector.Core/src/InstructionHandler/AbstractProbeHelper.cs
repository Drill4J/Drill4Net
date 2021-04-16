using System;
using System.Linq;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractProbeHelper
    {
        #region GenerateProbe
        public virtual string GenerateProbe(MethodContext ctx, CrossPoint point)
        {
            return point.BusinessIndex == -1 
                ? $"{GenerateProbePrefix(ctx, point)}{point.PointType}"
                : $"{GenerateProbePrefix(ctx, point)}{GenerateProbeData(point)}";
        }

        protected abstract string GenerateProbePrefix(MethodContext ctx, CrossPoint point);
        public abstract string GenerateProbeData(CrossPoint point);
        #endregion
        #region CrossPoint
        public virtual CrossPoint GetPoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var existingPoint = ctx.Method.Filter(typeof(CrossPoint), true)
                .Cast<CrossPoint>()
                .FirstOrDefault(a => a.PointId == localId.ToString());
            return existingPoint ?? CreateCrossPoint(ctx, pointType, localId);
        }

        protected virtual CrossPoint CreateCrossPoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var id = localId == -1 ? null : localId.ToString();
            var pointUid = Guid.NewGuid().ToString();
            var businessIndex = CalsBusinessIndex(ctx, localId);

            var point = new CrossPoint(pointUid, id, businessIndex, pointType)
            {
                //TODO: PDB data
            };
            return point;
        }
        #endregion

        internal virtual int CalsBusinessIndex(MethodContext ctx, int localIndex)
        {
            if (localIndex == -1) //TODO: what about this case?!!!
                return localIndex;
            var ind = localIndex;
            var method = ctx.Method;
            //go up to business method and sum the real index shift (business index)
            while (true)
            {
                var info = method.CGInfo;
                var caller = info?.Caller;
                if (caller == null)
                    break;
                var indexes = caller.CalleeIndexes;
                if (!indexes.ContainsKey(method.Fullname))
                    break;
                ind += indexes[method.Fullname];
                method = caller;
            }
            return ind;
        }
    }
}

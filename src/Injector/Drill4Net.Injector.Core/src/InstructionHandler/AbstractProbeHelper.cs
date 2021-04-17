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
            return $"{GenerateProbePrefix(ctx, point)}{GenerateProbeData(point)}";
        }

        protected abstract string GenerateProbePrefix(MethodContext ctx, CrossPoint point);
        protected abstract string GenerateProbeData(CrossPoint point);
        #endregion
        #region CrossPoint
        public virtual CrossPoint GetPoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var point = ctx.Method.Points
                .FirstOrDefault(a => a.PointType == pointType && a.PointId == localId.ToString()); //check for PointType need to use also
            if (point != null) 
                return point;
            point = CreateCrossPoint(ctx, pointType, localId);
            ctx.Method.AddChild(point);
            return point;
        }

        protected virtual CrossPoint CreateCrossPoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var pointUid = Guid.NewGuid().ToString();
            var businessIndex = CalcBusinessIndex(ctx, localId);
            var point = new CrossPoint(pointUid, localId.ToString(), businessIndex, pointType)
            {
                //TODO: PDB data
            };
            return point;
        }

        internal virtual int CalcBusinessIndex(MethodContext ctx, int localIndex)
        {
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
        #endregion
    }
}

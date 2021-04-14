using System;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractProbeHelper
    {
        #region GetProbeData
        public virtual string GetProbeData(MethodContext ctx)
        {
            return GetProbeData(ctx, CrossPointType.Unset, ctx.SourceIndex);
        }
        
        public virtual string GetProbeData(MethodContext ctx, CrossPointType pointType)
        {
            return GetProbeData(ctx, pointType, ctx.SourceIndex);
        }

        public virtual string GetProbeData(MethodContext ctx, CrossPointType pointType, int byIndex)
        {
            var point = CreateCrossPoint(ctx, pointType, byIndex);
            return GenerateProbeData(ctx, point);
        }
        #endregion

        protected abstract string GenerateProbeData(MethodContext ctx, CrossPoint point);

        protected virtual CrossPoint CreateCrossPoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var id = localId == -1 ? null : localId.ToString();
            var pointUid = Guid.NewGuid().ToString();
            var businessIndex = CalsBusinessIndex(ctx, localId);

            var point = new CrossPoint(pointUid, id, businessIndex, pointType)
            {
                //TODO: PDB data
            };
            ctx.Method.AddChild(point);
            
            return point;
        }

        internal virtual int CalsBusinessIndex(MethodContext ctx, int localIndex)
        {
            if (localIndex == -1) //TODO: what about tis case?!!!
                return localIndex;
            var ind = localIndex;
            var method = ctx.Method;
            while (true)
            {
                var info = method.CompilerGeneratedInfo;
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

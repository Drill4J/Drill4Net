using System;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractProbeHelper
    {     
        public virtual string PrepareProbeData(InjectorContext ctx, CrossPointType pointType, int localId, 
            Dictionary<string, object> data = null)
        {
            var id = localId == -1 ? null : localId.ToString();
            var pointUid = Guid.NewGuid().ToString();

            var point = new CrossPoint(pointUid, id, pointType)
            {
                //TODO: PDB data
            };
            ctx.TreeMethod.AddChild(point);

            return GenerateProbeData(ctx, point, pointUid, data);
        }

        protected abstract string GenerateProbeData(InjectorContext ctx, CrossPoint point, string pointUid,
            Dictionary<string, object> data);
    }
}

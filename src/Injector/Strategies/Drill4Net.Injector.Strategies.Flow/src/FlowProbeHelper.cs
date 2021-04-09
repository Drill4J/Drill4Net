using System.Collections.Generic;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowProbeHelper : AbstractProbeHelper
    {
        protected override string GenerateProbeData(InjectorContext ctx, CrossPoint point, string pointUid, 
            Dictionary<string, object> data)
        {
            var injMeth = ctx.TreeMethod;
            return $"{pointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^{point.PointType}_{point.PointId}"; 
        }
    }
}
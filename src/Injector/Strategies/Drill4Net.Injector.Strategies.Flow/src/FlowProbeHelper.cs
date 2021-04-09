using System.Collections.Generic;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Strategies.Flow
{
    public class FlowProbeHelper : AbstractProbeHelper
    {
        protected override string GenerateProbeData(InjectedMethod injMeth, CrossPoint point, string pointUid, 
            Dictionary<string, object> data)
        {
            return $"{pointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^{point.PointType}_{point.PointId}"; 
        }
    }
}
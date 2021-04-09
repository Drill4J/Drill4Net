using System.Collections.Generic;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockProbeHelper : AbstractProbeHelper
    {
        protected override string GenerateProbeData(InjectedMethod injMeth, CrossPoint point, string pointUid,
            Dictionary<string, object> data)
        {
            var moduleName = data?.ContainsKey(C.PROBE_KEY_MODULE) == true ? data[C.PROBE_KEY_MODULE] : "unknown";
            return $"{pointUid}^{moduleName}^{injMeth.BusinessMethod}^{point.PointType}_{point.PointId}";
        }
    }
}
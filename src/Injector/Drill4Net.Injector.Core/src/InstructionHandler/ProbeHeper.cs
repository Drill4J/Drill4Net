using System;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ProbeHeper
    {
        public string GetProbeData(InjectedMethod injMeth, string moduleName, CrossPointType pointType, int localId)
        {
            var id = localId == -1 ? null : localId.ToString();
            var pointUid = Guid.NewGuid();

            var crossPoint = new CrossPoint(pointUid.ToString(), id, pointType)
            {
                //PDB data
            };
            injMeth.AddChild(crossPoint);

            return $"{injMeth.BusinessMethod}^{moduleName}^{injMeth.Fullname}^{pointUid}^{pointType}_{id}";
        }
    }
}

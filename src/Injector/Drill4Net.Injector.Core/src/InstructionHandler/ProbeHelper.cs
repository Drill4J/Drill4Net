using System;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ProbeHelper
    {
        public virtual string GetProbeData(InjectedMethod injMeth, string moduleName, CrossPointType pointType, int index)
        {
            var id = index == -1 ? null : index.ToString();
            var pointUid = Guid.NewGuid();

            var crossPoint = new CrossPoint(pointUid.ToString(), id, pointType)
            {
                //PDB data
            };
            injMeth.AddChild(crossPoint);

            return $"{moduleName}^{injMeth.BusinessMethod}^{pointUid}^{pointType}_{id}";
        }
    }
}

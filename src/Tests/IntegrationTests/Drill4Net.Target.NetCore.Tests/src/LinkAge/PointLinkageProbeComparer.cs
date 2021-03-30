using System.Collections.Generic;

namespace Drill4Net.Target.NetCore.Tests
{
    public class PointLinkageProbeComparer : IComparer<PointLinkage>
    {
        int IComparer<PointLinkage>.Compare(PointLinkage x, PointLinkage y)
        {
            return x.Probe.CompareTo(y.Probe);
        }
    }
}

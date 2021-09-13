using System.Collections.Generic;

namespace Drill4Net.Target.Tests.Common
{
    public class PointLinkageProbeComparer : IComparer<PointLinkage>
    {
        int IComparer<PointLinkage>.Compare(PointLinkage x, PointLinkage y)
        {
            return x.Probe.CompareTo(y.Probe);
        }
    }
}

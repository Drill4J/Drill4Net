using System.Collections.Generic;

namespace Drill4Net.Admin.Requester
{
    public record TestToRunResponse
    {
        public Dictionary<string, List<TestToRunInfoResponse>> ByType { get; set; }
        public int TotalCount { get; set; }
    }
}

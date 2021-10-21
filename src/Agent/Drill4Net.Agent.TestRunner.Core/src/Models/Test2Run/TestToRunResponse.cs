using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    public record TestToRunResponse
    {
        public Dictionary<string, List<TestToRunInfoResponse>> ByType { get; set; }
        public int TotalCount { get; set; }
    }
}

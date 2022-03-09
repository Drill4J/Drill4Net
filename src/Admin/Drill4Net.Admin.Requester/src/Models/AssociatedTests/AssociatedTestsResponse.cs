using System.Collections.Generic;

namespace Drill4Net.Admin.Requester
{
    public record AssociatedTestsResponse
    {
        public List<AssociatedTest> Tests { get; set; } = new();
    }
}

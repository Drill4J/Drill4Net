using System;

namespace Drill4Net.Admin.Requester
{
    [Serializable]
    public record AssociatedTest
    {
        public string id { get; set; }
        public string type { get; set; }

        public string name { get; set; }

        public bool toRun { get; set; }

        public TestCoverage coverage { get; set; }

        public AssociatedTestOverview overview { get; set; }

        /************************************************************************************/

        public override string ToString()
        {
            return $"{id}: {name}";
        }
    }
}

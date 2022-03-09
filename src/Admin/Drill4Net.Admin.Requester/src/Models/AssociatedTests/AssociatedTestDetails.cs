using System;
using System.Collections.Generic;

namespace Drill4Net.Admin.Requester
{
    [Serializable]
    public record AssociatedTestDetails
    {
        public string engine { get; set; }
        public string path { get; set; }
        public string testName { get; set; }
        public AssociatedTestParams @params { get; set;}
        public Dictionary<string, string> metadata { get; set; } = new();

        /********************************************************************/

        public override string ToString()
        {
            return $"{engine}: {testName}@{path}";
        }
    }
}
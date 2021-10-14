using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class Test2RunInfo
    {
        public string name { get; set; }
        public TestResult result  { get; set; }
        public long startedAt { get; set; }
        public long finishedAt { get; set; }
        public Dictionary<string, object> metadata { get; set; }
    }
}

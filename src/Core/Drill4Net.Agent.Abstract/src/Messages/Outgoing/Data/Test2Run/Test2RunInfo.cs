using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    public class Test2RunInfo
    {
        public string Name { get; set; }
        public TestResult Result  { get; set; }
        public long StartedAt { get; set; }
        public long FinishedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class TestRun
    {
        public long startedAt { get; set; }
        public long finishedAt { get; set; }

        public List<Test2RunInfo> tests { get; set; } //array?

        /*************************************************************/

        public TestRun()
        {
            tests = new List<Test2RunInfo>();
        }

        /*************************************************************/

        public override string ToString()
        {
            return $"{CommonUtils.ConvertFromUnixTime(startedAt):G}-{CommonUtils.ConvertFromUnixTime(finishedAt):G}: {tests.Count}";
        }
    }
}

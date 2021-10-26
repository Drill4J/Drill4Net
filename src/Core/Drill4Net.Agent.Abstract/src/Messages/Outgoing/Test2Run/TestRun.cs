using Drill4Net.Common;
using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
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

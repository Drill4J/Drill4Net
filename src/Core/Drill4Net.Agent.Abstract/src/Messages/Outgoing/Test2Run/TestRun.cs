using System;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract.Transfer;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record TestRun : OutgoingMessage
    {
        /// <summary>
        /// May be any?
        /// </summary>
        public string name { get; set; }

        public long startedAt { get; set; }
        public long finishedAt { get; set; }
        public List<Test2RunInfo> tests { get; set; } //array?

        /****************************************************************/

        public TestRun(string name = null) : base(TestConstants.TEST_TOPIC_TESTS_ADD)
        {
            this.name = name;
            startedAt = CommonUtils.GetCurrentUnixTimeMs();
            finishedAt = startedAt;
        }

        /****************************************************************/

        public override string ToString()
        {
            return name;
        }
    }
}

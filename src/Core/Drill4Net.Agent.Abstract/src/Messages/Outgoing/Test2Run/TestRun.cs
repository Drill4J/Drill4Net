using System;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

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
            tests = new List<Test2RunInfo>();
        }

        /****************************************************************/

        public override string ToString()
        {
            return $"{name}: {tests.Count}";
        }
    }
}

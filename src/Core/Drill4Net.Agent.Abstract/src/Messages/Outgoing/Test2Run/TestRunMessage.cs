using System;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record TestRunMessage : OutgoingMessage
    {
        public TestRunPayload payload { get; set; }

        /****************************************************************/

        public TestRunMessage(string sessionId, TestRun run) : base(TestConstants.TEST_TOPIC_TESTS_ADD)
        {
            payload = new TestRunPayload(sessionId, run);
        }

        /****************************************************************/

        public override string ToString()
        {
            return payload != null ? payload.ToString() : ToString();
        }
    }
}

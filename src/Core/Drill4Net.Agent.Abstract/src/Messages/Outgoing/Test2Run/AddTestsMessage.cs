using System;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record AddTestsMessage : OutgoingMessage
    {
        public AddTestsPayload payload { get; set; }

        /****************************************************************/

        public AddTestsMessage(string sessionId, List<Test2RunInfo> tests):
            base(TestConstants.TEST_TOPIC_TESTS_ADD)
        {
            payload = new AddTestsPayload(sessionId, tests);
        }

        /****************************************************************/

        public override string ToString()
        {
            return payload != null ? payload.ToString() : ToString();
        }
    }
}

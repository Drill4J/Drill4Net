using Drill4Net.Common;
using System;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    public class TestRunPayload
    {
        public string sessionId { get; set; }

        public TestRun testRun { get; set; }

        /************************************************************************/

        public TestRunPayload(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentNullException(nameof(sessionId));
            this.sessionId = sessionId;
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"[{sessionId}] -> {testRun}";
        }
    }
}

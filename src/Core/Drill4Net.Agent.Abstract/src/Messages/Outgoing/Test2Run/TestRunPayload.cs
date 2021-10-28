using System;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class TestRunPayload
    {
        public string sessionId { get; set; }

        public TestRun testRun { get; set; }

        /************************************************************************/

        public TestRunPayload(string sessionId, TestRun run)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentNullException(nameof(sessionId));
            this.sessionId = sessionId;
            this.testRun = run  ?? throw new ArgumentNullException(nameof(run));
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"[{sessionId}] -> {testRun}";
        }
    }
}

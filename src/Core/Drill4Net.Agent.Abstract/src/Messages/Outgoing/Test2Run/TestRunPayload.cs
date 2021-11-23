using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class TestRunPayload
    {
        public string sessionId { get; set; }

        public List<Test2RunInfo> tests { get; set; }

        /************************************************************************/

        public TestRunPayload(string sessionId, List<Test2RunInfo> tests)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentNullException(nameof(sessionId));
            this.sessionId = sessionId;
            this.tests = tests ?? throw new ArgumentNullException(nameof(tests));
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"[{sessionId}] -> {(tests?.Count) ?? 0}";
        }
    }
}

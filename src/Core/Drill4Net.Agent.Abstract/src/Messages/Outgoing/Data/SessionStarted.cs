using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionStarted : AbstractSessionMessage
    {
        public string TestType { get; set; }
        public bool IsRealtime { get; set; }

        public SessionStarted() : base(AgentConstants.MESSAGE_OUT_SESSION_STARTED)
        {
        }
    }
}
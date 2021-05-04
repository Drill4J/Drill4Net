using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionStarted : AbstractSessionMessage
    {
        public string testType { get; set; }
        public bool isRealtime { get; set; }

        public SessionStarted() : base(AgentConstants.MESSAGE_OUT_SESSION_STARTED)
        {
        }
    }
}
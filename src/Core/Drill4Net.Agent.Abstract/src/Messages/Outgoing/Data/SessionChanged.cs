using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionChanged : AbstractOutgoingMessage
    {
        public string SessionId { get; set; }
        public int ProbeCount { get; set; }

        public SessionChanged() : base(AgentConstants.MESSAGE_OUT_SESSION_CHANGED)
        {
        }
    }
}
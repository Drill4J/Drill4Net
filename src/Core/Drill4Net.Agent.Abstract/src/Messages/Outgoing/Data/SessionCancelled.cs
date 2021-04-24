using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionCancelled : AbstractOutgoingMessage
    {
        public string SessionId { get; set; }
        public long Ts { get; set; }

        public SessionCancelled(string type) : base(AgentConstants.MESSAGE_OUT_SESSION_CANCELLED)
        {
        }
    }
}
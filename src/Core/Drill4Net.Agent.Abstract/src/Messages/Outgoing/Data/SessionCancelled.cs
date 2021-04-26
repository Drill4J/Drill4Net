using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionCancelled : AbstractMessage
    {
        public string SessionId { get; set; }
        public long Ts { get; set; }

        public SessionCancelled() : base(AgentConstants.MESSAGE_OUT_SESSION_CANCELLED)
        {
        }
    }
}
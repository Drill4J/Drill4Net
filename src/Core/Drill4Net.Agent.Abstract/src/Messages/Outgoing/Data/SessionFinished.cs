using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionFinished : AbstractOutgoingMessage
    {
        public string SessionId { get; set; }
        public long Ts { get; set; }

        public SessionFinished(string type) : base(AgentConstants.MESSAGE_OUT_SESSION_FINISHED)
        {
        }
    }
}
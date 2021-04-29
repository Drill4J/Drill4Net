using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionsCancelled : AbstractSomeSessionsMessage
    {
        public SessionsCancelled() : base(AgentConstants.MESSAGE_OUT_SESSION_ALL_CANCELLED)
        {
        }
    }
}
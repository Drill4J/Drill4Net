using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record SessionsCancelled: AbstractSomeSessionsMessage
    {
        public SessionsCancelled(): base(AgentConstants.MESSAGE_OUT_SESSION_ALL_CANCELLED)
        {
        }
    }
}
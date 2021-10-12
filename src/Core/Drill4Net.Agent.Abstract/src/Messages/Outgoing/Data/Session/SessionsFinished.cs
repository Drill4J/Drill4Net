using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record SessionsFinished : AbstractSomeSessionsMessage
    {
        public SessionsFinished(): base(AgentConstants.MESSAGE_OUT_SESSION_ALL_FINISHED)
        {
        }
    }
}
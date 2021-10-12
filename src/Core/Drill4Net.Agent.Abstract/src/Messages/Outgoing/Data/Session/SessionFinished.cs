using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record SessionFinished: AbstractSessionMessage
    {
        public SessionFinished(): base(AgentConstants.MESSAGE_OUT_SESSION_FINISHED)
        {
        }
    }
}
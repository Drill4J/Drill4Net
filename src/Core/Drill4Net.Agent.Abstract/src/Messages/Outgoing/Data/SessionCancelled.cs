using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionCancelled : AbstractSessionMessage
    {
        public SessionCancelled() : base(AgentConstants.MESSAGE_OUT_SESSION_CANCELLED)
        {
        }
    }
}
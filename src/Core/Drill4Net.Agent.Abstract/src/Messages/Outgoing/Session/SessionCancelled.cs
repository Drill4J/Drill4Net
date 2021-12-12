using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record SessionCancelled: AbstractSessionMessage
    {
        public SessionCancelled(): base(AgentConstants.MESSAGE_OUT_SESSION_CANCELLED)
        {
        }
    }
}
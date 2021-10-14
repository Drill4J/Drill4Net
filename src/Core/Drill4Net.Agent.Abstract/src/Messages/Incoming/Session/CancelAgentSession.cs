using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record CancelAgentSession : IncomingMessage
    {
        public AgentSessionPayload Payload { get; set; }

        /***************************************************************************/

        public CancelAgentSession() : base(AgentConstants.MESSAGE_IN_CANCEL_SESSION)
        {
        }
    }
}
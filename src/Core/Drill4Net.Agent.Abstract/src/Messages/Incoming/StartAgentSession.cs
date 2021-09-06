using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record StartAgentSession : IncomingMessage
    {
        public StartSessionPayload Payload { get; set; }

        /***************************************************************************/

        public StartAgentSession() : base(AgentConstants.MESSAGE_IN_START_SESSION)
        {
        }
    }
}
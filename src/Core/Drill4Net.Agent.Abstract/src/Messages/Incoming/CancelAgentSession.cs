using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class CancelAgentSession : IncomingMessage
    {
        public AgentSessionPayload Payload { get; set; }
    }
}
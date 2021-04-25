using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class StopAgentSession : IncomingMessage
    {
        public AgentSessionPayload Payload { get; set; }
    }
}
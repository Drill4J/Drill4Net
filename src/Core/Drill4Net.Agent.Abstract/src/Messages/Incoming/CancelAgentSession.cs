using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class CancelAgentSession
    {
        public AgentSessionPayload Payload { get; set; }
    }
}
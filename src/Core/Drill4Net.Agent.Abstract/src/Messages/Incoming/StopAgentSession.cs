using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class StopAgentSession
    {
        public AgentSessionPayload Payload { get; set; }
    }
}
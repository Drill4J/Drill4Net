using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class StartAgentSession
    {
        public StartSessionPayload Payload { get; set; }
    }
}
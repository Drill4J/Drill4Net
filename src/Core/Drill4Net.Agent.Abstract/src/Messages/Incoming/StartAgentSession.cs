using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class StartAgentSession : IncomingMessage
    {
        public StartSessionPayload Payload { get; set; }
    }
}
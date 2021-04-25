using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class AgentSessionPayload : IncomingMessage
    {
        public string SessionId { get; set; }
    }
}
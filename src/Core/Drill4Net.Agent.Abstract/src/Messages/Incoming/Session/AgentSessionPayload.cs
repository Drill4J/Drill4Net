using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record AgentSessionPayload
    {
        public string SessionId { get; set; }
    }
}
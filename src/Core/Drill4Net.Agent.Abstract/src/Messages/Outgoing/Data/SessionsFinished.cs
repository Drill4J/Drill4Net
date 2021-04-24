using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionsFinished : AbstractOutgoingMessage
    {
        public List<string> IDs { get; set; }
        public long Ts { get; set; }

        public SessionsFinished(string type) : base(AgentConstants.MESSAGE_OUT_SESSION_ALL_FINISHED)
        {
        }
    }
}
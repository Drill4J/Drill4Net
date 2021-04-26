using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionsCancelled : AbstractMessage
    {
        public List<string> IDs { get; set; }
        public long Ts { get; set; }

        public SessionsCancelled() : base(AgentConstants.MESSAGE_OUT_SESSION_ALL_CANCELLED)
        {
        }
    }
}
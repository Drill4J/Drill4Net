﻿using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class StopAgentSession : IncomingMessage
    {
        public AgentSessionPayload Payload { get; set; }

        public StopAgentSession() : base(AgentConstants.MESSAGE_IN_STOP_SESSION)
        {
        }
    }
}
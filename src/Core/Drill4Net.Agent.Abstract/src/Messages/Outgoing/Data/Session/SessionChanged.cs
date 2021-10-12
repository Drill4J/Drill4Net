﻿using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record SessionChanged: OutgoingMessage
    {
        public string sessionId { get; set; }
        public int probeCount { get; set; }

        /***************************************************************/

        public SessionChanged(): base(AgentConstants.MESSAGE_OUT_SESSION_CHANGED)
        {
        }
    }
}
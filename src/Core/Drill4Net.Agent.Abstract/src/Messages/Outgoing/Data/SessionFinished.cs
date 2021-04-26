using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class SessionFinished : AbstractMessage
    {
        public string SessionId { get; set; }
        public long Ts { get; set; }

        public SessionFinished() : base(AgentConstants.MESSAGE_OUT_SESSION_FINISHED)
        {
        }
    }
}
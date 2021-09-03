using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class SessionStarted : AbstractSessionMessage
    {
        public string testType { get; set; }
        public bool isRealtime { get; set; }

        /*****************************************************************************/

        public SessionStarted() : base(AgentConstants.MESSAGE_OUT_SESSION_STARTED)
        {
        }

        /*****************************************************************************/

        public override string ToString()
        {
            return $"Session={sessionId}, testType={testType}, isRealTime={isRealtime}";
        }
    }
}
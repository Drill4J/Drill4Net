using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public class Initialized : OutgoingMessage
    {
        public string msg { get; set; }

        public Initialized() : base(AgentConstants.MESSAGE_OUT_INITIALIZED)
        {
        }
    }
}
using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public abstract record AbstractSessionMessage : OutgoingMessage
    {       
        public string sessionId { get; set; }

        public long ts { get; set; }

        protected AbstractSessionMessage(string type) : base(type)
        {
        }
    }
}

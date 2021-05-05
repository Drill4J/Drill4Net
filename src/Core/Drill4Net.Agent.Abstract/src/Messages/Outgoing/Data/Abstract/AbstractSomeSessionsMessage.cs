using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class AbstractSomeSessionsMessage : OutgoingMessage
    {     
        public List<string> ids { get; set; }

        public long ts { get; set; }

        protected AbstractSomeSessionsMessage(string type) : base(type)
        {
        }
    }
}

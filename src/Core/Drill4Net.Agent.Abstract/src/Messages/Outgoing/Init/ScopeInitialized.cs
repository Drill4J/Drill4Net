using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public record ScopeInitialized: OutgoingMessage
    {
        public string id { get; set; }
        public string name { get; set; }
        public string prevId { get; set; }
        public long ts { get; set; }
        
        /*************************************************************************/
        
        public ScopeInitialized(string id, string name, string prevId, long ts):
            base(AgentConstants.MESSAGE_OUT_SCOPE_INITIALIZED)
        {
            this.id = id;
            this.name = name;
            this.prevId = prevId;
            this.ts = ts;
        }
    }
}
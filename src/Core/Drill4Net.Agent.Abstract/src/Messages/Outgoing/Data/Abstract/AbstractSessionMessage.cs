namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class AbstractSessionMessage : AbstractMessage
    {       
        public string sessionId { get; set; }

        public long ts { get; set; }

        protected AbstractSessionMessage(string type) : base(type)
        {
        }
    }
}

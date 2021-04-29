using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class AbstractMessage
    {
        public string type { get; set; }
        
        protected AbstractMessage(string type)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class BaseMessage //must be CONCRETE type due itself direct deserialization 
    {
        public string type { get; set; }

        public BaseMessage() { }

        protected BaseMessage(string type)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
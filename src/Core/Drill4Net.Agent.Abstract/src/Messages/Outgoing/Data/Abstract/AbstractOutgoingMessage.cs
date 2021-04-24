using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public abstract class AbstractOutgoingMessage
    {
        public string Type { get; set; }
        
        protected AbstractOutgoingMessage(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
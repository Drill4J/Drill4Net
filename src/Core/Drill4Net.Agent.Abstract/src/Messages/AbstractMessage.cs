using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public abstract class AbstractMessage
    {
        public string Type { get; set; }
        
        protected AbstractMessage(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
using System;

namespace Drill4Net.Agent.Messaging
{
    [Serializable]
    public class Probe
    {
        public string Context { get; set; }
        public string Data { get; set; }

        public override string ToString()
        {
            return $"data=[{Data}]; Context=[{Context}]";
        }
    }
}

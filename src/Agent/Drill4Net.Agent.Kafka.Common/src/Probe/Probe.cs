using System;

namespace Drill4Net.Agent.Kafka.Common
{
    [Serializable]
    public class Probe
    {
        public string Context { get; set; }
        public string Data { get; set; }
    }
}

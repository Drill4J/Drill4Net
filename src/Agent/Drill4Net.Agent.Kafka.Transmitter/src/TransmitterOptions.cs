using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class TransmitterOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public List<string> Topics { get; set; }

        public TransmitterOptions()
        {
            Servers = new();
            Topics = new();
        }
    }
}

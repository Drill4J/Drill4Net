using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class TransmitterOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public string Topic { get; set; }
    }
}

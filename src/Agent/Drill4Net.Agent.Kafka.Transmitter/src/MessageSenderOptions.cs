using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class MessageSenderOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public List<string> Topics { get; set; }

        /************************************************/

        public MessageSenderOptions()
        {
            Servers = new();
            Topics = new();
        }
    }
}

using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Transport
{
    public class MessageReceiverOptions : BaseMessageOptions
    {
        public string GroupId { get; set; }
    }
}

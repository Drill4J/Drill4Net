using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Kafka
{
    internal class CommandSerializer : ISerializer<Command>
    {
        public byte[] Serialize(Command data, SerializationContext context)
        {
            var ar = Serializer.ToArray(data);
            var compressed = Compressor.Compress(ar);
            return compressed;
        }
    }
}

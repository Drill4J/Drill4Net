using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class ProbeSerializer : ISerializer<Probe>
    {
        public byte[] Serialize(Probe data, SerializationContext context)
        {
            var ar = Serializer.ToArray(data);
            var compressed = Compressor.Compress(ar);
            return compressed;
        }
    }
}

using System;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class ProbeDeserializer : IDeserializer<Probe>
    {
        public Probe Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            //TODO: compressor for using ReadOnlySpan !!!
            var ar = Compressor.Decompress(data.ToArray());
            var probe = Serializer.FromArray<Probe>(ar);
            return probe;
        }
    }
}

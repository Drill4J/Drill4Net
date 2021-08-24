using System.Collections.Specialized;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class StringDictionarySerializer : ISerializer<StringDictionary>
    {
        public byte[] Serialize(StringDictionary data, SerializationContext context)
        {
            var ar = Serializer.ToArray(data);
            var compressed = Compressor.Compress(ar);
            return compressed;
        }
    }
}

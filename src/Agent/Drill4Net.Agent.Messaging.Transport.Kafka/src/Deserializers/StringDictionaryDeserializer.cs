using System;
using System.Collections.Specialized;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class StringDictionaryDeserializer : IDeserializer<StringDictionary>
    {
        public StringDictionary Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            //TODO: compressor for using ReadOnlySpan !!!
            var ar = Compressor.Decompress(data.ToArray());
            var probe = Serializer.FromArray<StringDictionary>(ar);
            return probe;
        }
    }
}

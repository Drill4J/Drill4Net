using System;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    internal class CommandDeserializer : IDeserializer<Command>
    {
        public Command Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            //TODO: compressor for using ReadOnlySpan !!!
            var ar = Compressor.Decompress(data.ToArray());
            var command = Serializer.FromArray<Command>(ar);
            return command;
        }
    }
}

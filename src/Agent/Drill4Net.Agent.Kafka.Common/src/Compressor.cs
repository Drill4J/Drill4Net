using System;
using K4os.Compression.LZ4;

namespace Drill4Net.Agent.Kafka.Common
{
    //https://github.com/MiloszKrajewski/K4os.Compression.LZ4

    public static class Compressor
    {
        public static byte[] Compress(byte[] source)
        {
            var buffer = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
            var encodedLength = LZ4Codec.Encode(source, 0, source.Length, buffer, 0, buffer.Length, LZ4Level.L03_HC);
            if (encodedLength < 0)
                throw new InvalidOperationException("Compress failed");
            var destination = new byte[encodedLength];
            Array.Copy(buffer, 0, destination, 0, encodedLength);
            return destination;
        }

        public static byte[] Decompress(byte[] source)
        {
            //Decode(byte[] source, int sourceOffset, int sourceLength, byte[] target, int targetOffset, int targetLength);
            var buffer = new byte[source.Length * 255]; // to be safe
            var decodedLength = LZ4Codec.Decode(source, 0, source.Length, buffer, 0, buffer.Length);
            if (decodedLength < 0)
                throw new InvalidOperationException("Decompress' buffer is too small");
            var destination = new byte[decodedLength];
            Array.Copy(buffer, 0, destination, 0, decodedLength);
            return destination;
        }
    }
}

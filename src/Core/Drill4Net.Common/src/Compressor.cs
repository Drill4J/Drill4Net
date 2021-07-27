using System;
using K4os.Compression.LZ4;

namespace Drill4Net.Common
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

        public static byte[] Decompress(byte[] source, int knownSize = 0)
        {
            var buffer = new byte[knownSize == 0 ? source.Length * 255 : knownSize]; // to be safe
            var decodedLength = LZ4Codec.Decode(source, 0, source.Length, buffer, 0, buffer.Length);
            if (decodedLength < 0)
                throw new InvalidOperationException("Decompress' buffer is too small");
            if (knownSize > 0)
                return buffer;
            var destination = new byte[decodedLength];
            Array.Copy(buffer, 0, destination, 0, decodedLength);
            return destination;
        }
    }
}

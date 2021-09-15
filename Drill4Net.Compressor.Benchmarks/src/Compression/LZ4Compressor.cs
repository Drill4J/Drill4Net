using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Compressor.Benchmarks.Compression
{
    public static class LZ4Compressor
    {
        public static byte[] Compress(byte[] data, LZ4Level level)
        {
            var buffer = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
            var encodedLength = LZ4Codec.Encode(data, 0, data.Length, buffer, 0, buffer.Length, level);
            if (encodedLength < 0)
                throw new InvalidOperationException("Compression failed");
            var compressedData = new byte[encodedLength];
            Array.Copy(buffer, 0, compressedData, 0, encodedLength);
            return compressedData;
        }
    }
}

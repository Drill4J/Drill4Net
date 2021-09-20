using System.IO;
using System.IO.Compression;

namespace Drill4Net.Compressor.Benchmarks.Compressors
{
    internal static class DeflateCompressor
    {
        internal static byte[] CompressData(byte[] data, CompressionLevel compressionLevel)
        {
            MemoryStream compressedData = new();
            using (DeflateStream dstream = new(compressedData, compressionLevel))
            {
                dstream.Write(data, 0, data.Length);
            }
            return compressedData.ToArray();
        }
    }
}

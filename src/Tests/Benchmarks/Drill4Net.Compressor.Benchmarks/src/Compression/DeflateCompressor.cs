using System.IO;
using System.IO.Compression;

namespace Drill4Net.Compressor.Benchmarks.Compression
{
    internal static class DeflateCompressor
    {
        internal static byte[] CompressData(byte[] data, CompressionLevel compressionLevel)
        {
            MemoryStream compressedData = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(compressedData, compressionLevel))
            {
                dstream.Write(data, 0, data.Length);
            }
            return compressedData.ToArray();           
        }
    }
}

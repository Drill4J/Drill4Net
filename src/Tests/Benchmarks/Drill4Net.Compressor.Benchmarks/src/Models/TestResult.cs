namespace Drill4Net.Compressor.Benchmarks.Models
{
    internal class TestResult
    {
        internal ModelTypes DataType { get; set; }
        internal double MinTime { get; set; } = 0;
        internal double MaxTime { get; set; } = 0;
        internal double AvgTime { get; set; } = 0;
        internal double MinCompressionRate { get; set; } = 0;
        internal double MaxCompressionRate { get; set; } = 0;
        internal double AvgCompressionRate { get; set; } = 0;
        internal double MinMemory { get; set; } = 0;
        internal double MaxMemory { get; set; } = 0;
        internal double AvgMemory { get; set; } = 0;
        internal CompressorTypes CompressorType { get; set; }
        internal string CompressLevel { get; set; } = string.Empty;
    }
}

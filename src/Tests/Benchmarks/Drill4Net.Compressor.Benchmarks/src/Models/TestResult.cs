using System;
using Drill4Net.Compressor.Benchmarks.Enums;

namespace Drill4Net.Compressor.Benchmarks.src.Models
{
    internal class TestResult
    {
        internal double MinTime { get; set; } = 0;
        internal double MaxTime { get; set; } = 0;
        internal double AvgTime { get; set; } = 0;
        internal double MinCompressionRate { get; set; } = 0;
        internal double MaxCompressionRate { get; set; } = 0;
        internal double AvgCompressionRate { get; set; } = 0;
        internal double MinMemory { get; set; } = 0;
        internal double MaxMemory { get; set; } = 0;
        internal double AvgMemory { get; set; } = 0;
        internal string CompressorName { get; set; } = String.Empty;
        internal string CompressLevel { get; set; } = String.Empty;
        internal DataTypes DataType { get; set; }
    }
}

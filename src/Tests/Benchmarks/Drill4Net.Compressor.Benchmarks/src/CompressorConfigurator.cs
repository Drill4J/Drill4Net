﻿using System;

namespace Drill4Net.Compressor.Benchmarks
{
    internal static class CompressorConfigurator
    {
        internal static readonly Random Rnd = new(DateTime.Now.Millisecond);
        internal const int DATA_COUNT = 100;
        internal const string CFG_NAME = "inj_Std.yml";
        internal const int ITERATIONS = 250;
        internal const int ITERATIONS_INJ = 250;
        internal const string BENCHMARK_LOG_PATH = @"logs\benchmarkLog.txt";
        internal const string LOG_PATH = @"logs\log.txt";

        //Table constants  
        internal const string COMPRESSOR = "Compressor";
        internal const string COMPRESSOR_LEVEL = "Comression Level";
        internal const string DATA_TYPE = "Data Type";

        internal const string MIN_TIME = "MinTime(msec)";
        internal const string MAX_TIME = "MaxTime(msec)";
        internal const string AVG_TIME = "AvgTime(msec)";

        internal const string MIN_COMR_RATE = "MinCompressionRate(%)";
        internal const string MAX_COMR_RATE = "MaxCompressionRate(%)";
        internal const string AVG_COMR_RATE = "AvgCompressionRate(%)";

        internal const string MIN_MEMORY_USAGE = "MinMemoryUsage(MByte)";
        internal const string MAX_MEMORY_USAGE = "MaxMemoryUsage(MByte)";
        internal const string AVG_MEMORY_USAGE = "AvgMemoryUsage(MByte)";

        internal const string ALL_TESTED = "All tested";
        internal const string SEPARATOR = "****************************************************";

    }
}

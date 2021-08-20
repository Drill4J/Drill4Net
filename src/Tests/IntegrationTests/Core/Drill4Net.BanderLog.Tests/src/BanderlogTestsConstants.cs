using System;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Constants for Banderlog Tests
    /// </summary>
    static class BanderlogTestsConstants
    {
        private const int _logLengh = 1000;
        public const int LOG_LINE_COUNT = 50000;
        public const string LOG_PATH = "BanderLog.txt";
        public const string LOG_PATH_THREADS = "Threads_BanderLog.txt";
        public static readonly string LOG_STRING = new('a', _logLengh);
        public static readonly string[] LOG_PATH_SINKS = new string[2]
        {
            "FileSink1_BanderLog.txt",
            "FileSink2_BanderLog.txt"
        };
    }
}

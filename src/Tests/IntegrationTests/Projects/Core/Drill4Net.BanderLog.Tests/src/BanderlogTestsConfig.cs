﻿using System.IO;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Constants for Banderlog Tests
    /// </summary>
    static class BanderlogTestsConfig
    {
        public const int LOG_LINE_COUNT = 50000;
        public static readonly string LOG_STRING = new('a', 1000);
        public static readonly string TEMP_PATH = Path.Combine(Path.GetTempPath(), "Drill4Net");
    }
}

using System;
using System.IO;

namespace Drill4Net.Common
{
    /// <summary>
    /// File logger for benchmarks
    /// </summary>
    public class BenchmarkFileLogger : IBenchmarkLogger
    {
        public string FilePath { get; }

        /**************************************************************/
        public BenchmarkFileLogger(string filePath)
        {
            FilePath = filePath;
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        /**************************************************************/

        public void WriteBenchmarkToLog(string msg)
        {
            File.AppendAllText(FilePath, msg + Environment.NewLine);
        }
    }
}

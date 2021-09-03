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
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }
        /**************************************************************/

        public void WriteBenchmarkToLog(string msg)
        {
            File.AppendAllText(FilePath, msg + Environment.NewLine);
        }
    }
}

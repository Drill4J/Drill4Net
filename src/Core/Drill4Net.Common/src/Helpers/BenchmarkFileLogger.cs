using System;
using System.IO;
using Drill4Net.Injector.App.Helpers.Interfaces;

namespace Drill4Net.Common.Helpers
{
    //File logger for benchmarks
    public class BenchmarkFileLogger : IBenchmarkLogger
    {
        private string _filePath;
        public string FilePath { get => _filePath; }

        /**************************************************************/
        public BenchmarkFileLogger(string filePath)
        {
            _filePath = filePath;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }
        /**************************************************************/

        public void WriteBenchmarkToLog(string msg)
        {
            File.AppendAllText(_filePath, msg + Environment.NewLine);
        }
    }
}

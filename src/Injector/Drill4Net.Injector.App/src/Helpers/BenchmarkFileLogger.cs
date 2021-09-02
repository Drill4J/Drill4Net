using System;
using System.IO;
using Drill4Net.Injector.App.Helpers.Interfaces;

namespace Drill4Net.Injector.App.Helpers
{
    //File logger for benchmarks
    internal class BenchmarkFileLogger : IBenchmarkLogger
    {
        private string _filePath;
        public string FilePath { get => _filePath; }

        /**************************************************************/
        internal BenchmarkFileLogger(string filePath)
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

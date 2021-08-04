using Drill4Net.BanderLog.Sinks.File;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.Testing
{
    public class LoggerTest
    {
        private const int logLengh=1000;
        private const int _logLineCount = 1000000;
        private string _logString=new string ('a', logLengh);
        private string _logPath = "BanderLog_Log.txt";

        public Logger InitializeLogger(string fileName)
        {
            var logBld = new LogBuilder();
            var logger = logBld.AddSink(new FileSink(fileName)).Build();
            return logger;
        }

        public void WriteLog(string fileName)
        {
            var logger = InitializeLogger(_logPath);

            for (var i = 1; i <= _logLineCount; i++)
            {
                logger.Log(LogLevel.Information, $"{i}_{_logString}");
            }
            logger.Flush();

        }
        public void WriteLog(Logger logger)
        {
            for (var i = 1; i <= _logLineCount; i++)
            {
                logger.Log(LogLevel.Information, $"{i}_{_logString}");
            }

        }
        [Fact]
        public void Test1()
        {
            File.Delete(_logPath);
            WriteLog(_logPath);
            int lineCounter = 0;
            string logLine;
            System.Threading.Thread.Sleep(5000);
            var file =
                new System.IO.StreamReader(_logPath);
            while ((logLine = file.ReadLine()) != null)
            {
                int actualLineNumber;
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|")+1, logLine.LastIndexOf("_")- logLine.LastIndexOf("|")-1);
                int.TryParse(lineNumberInLog, out actualLineNumber);
                //Check the fact that the lines do not change their order
                //(it's not terrible if it changes within small limits)
                Assert.True(lineCounter - 1 <= actualLineNumber && actualLineNumber <= lineCounter + 1);
                //The content is not distorted (at least the last line).
                Assert.EndsWith(_logString, logLine);
                lineCounter++;
            }
            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(_logLineCount, lineCounter);

        }


    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.BanderLog.Sinks;

namespace Drill4Net.BanderLog.Testing
{
    public class LoggerTest
    {
        private const int logLengh = 1000;
        private const int _logLineCount = 50000;
        private readonly string _logString = new('a', logLengh);
        private readonly string _logPath = "BanderLog.txt";

        /**********************************************************************************/

        public AbstractSink InitializeLogger(string fileName)
        {
            return FileSinkCreator.CreateSink(fileName);
        }

        [Fact]
        public void OneThreadOneLoggerTest()
        {
            //arrange
            if(File.Exists(_logPath))
                File.Delete(_logPath);
            Thread.Sleep(50); //??
            var logger = InitializeLogger(_logPath);

            //act
            WriteLog(logger);
            logger.Shutdown();

            //assert
            int lineCounter = 0;
            string logLine;
            using var file = new System.IO.StreamReader(_logPath);
        
            while ((logLine = file.ReadLine()) != null)
            {
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
                if (!int.TryParse(lineNumberInLog, out int actualLineNumber))
                    continue;

                //Check the fact that the lines do not change their order
                //(it's not terrible if it changes within small limits)
                Assert.True(lineCounter <= actualLineNumber && actualLineNumber <= lineCounter + 1);

                //The content is not distorted (at least the last line).
                Assert.EndsWith(_logString, logLine);
                lineCounter++;
            }
            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(_logLineCount, lineCounter);
        }

        [Fact]
        public void ParallelThreadsOneLoggerTest()
        {
            //arrange
            var fileName = $"Threads_{_logPath}";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var logger = InitializeLogger($"Threads_{_logPath}");

            //act
            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => WriteLog(logger, "thread_1_"));
            tasks[1] = Task.Run(() => WriteLog(logger, "thread_2_"));

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("An exception occurred:");
                foreach (var ex in ae.Flatten().InnerExceptions)
                    Console.WriteLine(" {0}", ex.Message);
            }
            finally
            {
                logger.Shutdown();
            }

            //assert
            using var file = new System.IO.StreamReader($"Threads_{_logPath}");
            string logLine;
            int lineCounterThread1 = 0;
            int lineCounterThread2 = 0;

            while ((logLine = file.ReadLine()) != null)
            {
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
                int.TryParse(lineNumberInLog, out int actualLineNumber);
                if (logLine.Contains("thread_1_"))
                {
                    //Check the fact that the lines do not change their order
                    //(it's not terrible if it changes within small limits)
                    Assert.True(lineCounterThread1 <= actualLineNumber && actualLineNumber <= lineCounterThread1 + 1);

                    //The content is not distorted (at least the last line).
                    Assert.EndsWith(_logString, logLine);
                    lineCounterThread1++;
                }

                if (logLine.Contains("thread_2_"))
                {
                    Assert.True(lineCounterThread2 <= actualLineNumber && actualLineNumber <= lineCounterThread2 + 1);
                    Assert.EndsWith(_logString, logLine);

                    lineCounterThread2++;
                }
            }

            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(_logLineCount, lineCounterThread1);
            Assert.Equal(_logLineCount, lineCounterThread2);
        }

        [Fact]
        public void ParallelThreadsTwoLoggersTest()
        {
            //arrange
            var fileName = $"Threads2Loggers_{_logPath}";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var logger1 = InitializeLogger($"Threads2Loggers_{_logPath}");
            var logger2 = InitializeLogger($"Threads2Loggers_{_logPath}");

            //act
            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => WriteLog(logger1, "thread_1_"));
            tasks[1] = Task.Run(() => WriteLog(logger2, "thread_2_"));

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("An exception occurred:");
                foreach (var ex in ae.Flatten().InnerExceptions)
                    Console.WriteLine("   {0}", ex.Message);
            }
            finally
            {
                logger1.Shutdown();
            }

            //assert
            using var file = new System.IO.StreamReader($"Threads2Loggers_{_logPath}");
            string logLine;
            int lineCounterThread1 = 0;
            int lineCounterThread2 = 0;

            while ((logLine = file.ReadLine()) != null)
            {
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
                if (!int.TryParse(lineNumberInLog, out int actualLineNumber))
                    continue;

                if (logLine.Contains("thread_1_"))
                {
                    //Check the fact that the lines do not change their order
                    //(it's not terrible if it changes within small limits)
                    Assert.True(lineCounterThread1 <= actualLineNumber && actualLineNumber <= lineCounterThread1 + 1);

                    //The content is not distorted (at least the last line).
                    Assert.EndsWith(_logString, logLine);
                    lineCounterThread1++;
                }

                if (logLine.Contains("thread_2_"))
                {
                    Assert.True(lineCounterThread2 <= actualLineNumber && actualLineNumber <= lineCounterThread2 + 1);
                    Assert.EndsWith(_logString, logLine);

                    lineCounterThread2++;
                }
            }

            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(_logLineCount, lineCounterThread1);
            Assert.Equal(_logLineCount, lineCounterThread2);
        }

        private void WriteLog(AbstractSink logger, string additionalInfo = "")
        {
            for (var i = 1; i <= _logLineCount; i++)
            {
                logger.Log(LogLevel.Information, $"{i}_{additionalInfo}{_logString}");
            }
        }
    }
}

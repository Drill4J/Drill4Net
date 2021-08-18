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
        private const int logLengh = 1000;
        private const int _logLineCount = 1000000;
        private string _logString = new string('a', logLengh);
        private string _logPath = "BanderLog.txt";

        public Logger InitializeLogger(string fileName)
        {
            var logBld = new LogBuilder();
            var logger = logBld.AddSink(FileSinkCreator.CreateSink(fileName)).Build();
            return logger;
        }


        public void WriteLog(Logger logger, string additionalInfo = "")
        {
            for (var i = 1; i <= _logLineCount; i++)
            {
                logger.Log(LogLevel.Information, $"{i}_{additionalInfo}{_logString}");
            }
        }
        [Fact]
        public async void OneThreadOneLoggerTestAsync()
        {
            File.Delete(_logPath);
            System.Threading.Thread.Sleep(50);
            var logger = InitializeLogger(_logPath);
            WriteLog(logger);
            await  logger.Flush();
            //await Task.Delay(30000);
            int lineCounter = 0;
            string logLine;
            using var file =
                new System.IO.StreamReader(_logPath);
            while ((logLine = file.ReadLine()) != null)
            {
                int actualLineNumber;
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
                int.TryParse(lineNumberInLog, out actualLineNumber);
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
        public async void ParallelThreadsOneLoggerTest()
        {
            File.Delete($"Threads_{_logPath}");
            var logger = InitializeLogger($"Threads_{_logPath}");
            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => WriteLog(logger, "thread_1_"));
            tasks[1] = Task.Run(() => WriteLog(logger, "thread_2_"));
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("An exception occurred.");
                foreach (var ex in ae.Flatten().InnerExceptions)
                    Console.WriteLine(" {0}", ex.Message);

            }
            finally
            {

                await logger.Flush();
            }
            using var file =
                new System.IO.StreamReader($"Threads_{_logPath}");
            string logLine;
            int lineCounterThread1 = 0;
            int lineCounterThread2 = 0;
            while ((logLine = file.ReadLine()) != null)
            {
                int actualLineNumber;
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
                int.TryParse(lineNumberInLog, out actualLineNumber);
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
                    //Check the fact that the lines do not change their order
                    //(it's not terrible if it changes within small limits)
                    Assert.True(lineCounterThread2 <= actualLineNumber && actualLineNumber <= lineCounterThread2 + 1);
                    //The content is not distorted (at least the last line).
                    Assert.EndsWith(_logString, logLine);
                    lineCounterThread2++;
                }

            }
            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(_logLineCount, lineCounterThread1);
            Assert.Equal(_logLineCount, lineCounterThread2);

        }
        [Fact]
        public async void ParallelThreadsTwoLoggersTest()
        {
            File.Delete($"Threads2Loggers_{_logPath}");
            var logger1 = InitializeLogger($"Threads2Loggers_{_logPath}");
            var logger2 = InitializeLogger($"Threads2Loggers_{_logPath}");
            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => WriteLog(logger1, "thread_1_"));
            tasks[1] = Task.Run(() => WriteLog(logger2, "thread_2_"));
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("An exception occurred.");
                foreach (var ex in ae.Flatten().InnerExceptions)
                    Console.WriteLine("   {0}", ex.Message);

            }
            finally
            {
                
                await logger1.Flush();
            }
            using var file =
                new System.IO.StreamReader($"Threads2Loggers_{_logPath}");
            string logLine;
            int lineCounterThread1 = 0;
            int lineCounterThread2 = 0;
            while ((logLine = file.ReadLine()) != null)
            {
                int actualLineNumber;
                var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
                int.TryParse(lineNumberInLog, out actualLineNumber);
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
                    //Check the fact that the lines do not change their order
                    //(it's not terrible if it changes within small limits)
                    Assert.True(lineCounterThread2 <= actualLineNumber && actualLineNumber <= lineCounterThread2 + 1);
                    //The content is not distorted (at least the last line).
                    Assert.EndsWith(_logString, logLine);
                    lineCounterThread2++;
                }

            }
            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(_logLineCount, lineCounterThread1);
            Assert.Equal(_logLineCount, lineCounterThread2);

        }



    }
}

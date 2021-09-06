using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Drill4Net.Common;
using Drill4Net.BanderLog.Sinks.File;
using Helper = Drill4Net.BanderLog.Tests.BanderlogTestsHelper;
using Const = Drill4Net.BanderLog.Tests.BanderlogTestsConfig;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Tests for Banderlog Logger
    /// </summary>
    public class LoggerTests
    {
        private BanderLog.LogManager InitializeLogger(string[] fileNames)
        {
            var logBld = new LogBuilder();

            foreach (var fileName in fileNames)
            {
                logBld.AddSink(FileSinkCreator.CreateSink(fileName));
            }
            
            return logBld.Build();
        }

        private string[] PrepareFilePaths(int count )
        {
            string[] filePaths = new string[2];
            for(var i = 0; i < count; i++)
            {
                var logName = Path.GetRandomFileName();
                filePaths[i] = Path.Combine(Const.TEMP_PATH, logName);
            }
            return filePaths;
        }

        [Fact]
        public void ParallelThreadsOneLoggerTest()
        {
            var filePaths = PrepareFilePaths(2);
            try
            {
                //arrange
                var logger = InitializeLogger(filePaths);

                //act
                var sinks = logger.GetSinks();
                Task[] tasks = new Task[2]
                {
                    new Task(() => Helper.WriteLog(sinks[0])),
                    new Task(() => Helper.WriteLog(sinks[1]))
                };

                foreach (var t in tasks)
                    t.Start();

                try
                {
                    Task.WaitAll(tasks);
                }
                catch (AggregateException ae)
                {
                    Console.WriteLine(CommonUtils.GetExceptionDescription(ae));
                }
                finally
                {
                    logger.Shutdown();
                }

                //assert
                foreach (var filePath in filePaths)
                {
                    var lineCounter = 0;
                    var logLinesSinks = File.ReadAllLines(filePath);
                    Assert.Equal(Const.LOG_LINE_COUNT, logLinesSinks.Length);

                    foreach (var logLine in logLinesSinks)
                    {
                        var actualLineNumber = Helper.GetLineNumber(logLine);
                        Helper.AssertLogLine(lineCounter, actualLineNumber, logLine);
                        lineCounter++;
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(CommonUtils.GetExceptionDescription(ex));
            }
            finally
            {
                foreach (var filePath in filePaths)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }
        }
    }
}

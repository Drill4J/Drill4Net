using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.BanderLog.Sinks.File;
using Xunit;
using Const = Drill4Net.BanderLog.Tests.BanderlogTestsConstants;
using Helper = Drill4Net.BanderLog.Tests.BanderlogTestsHelper;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Tests for Banderlog Logger
    /// </summary>
    public class LoggerTests
    {
        private BanderLog.Logger InitializeLogger(string[] fileNames)
        {
            var logBld = new LogBuilder();

            foreach (var fileName in fileNames)
            {
                logBld.AddSink(FileSinkCreator.CreateSink(fileName));
            }
            
            return logBld.Build();
        }

        [Fact]
        public void ParallelThreadsOneLoggerTest()
        {
            //arrange
            foreach (var fileName in Const.LOG_PATH_SINKS)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            var logger = InitializeLogger(Const.LOG_PATH_SINKS);

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
                BanderlogTestsUtils.WriteAggregateException(ae);
            }
            finally
            {
                logger.Shutdown();
            }

            //assert
            foreach (var fileName in Const.LOG_PATH_SINKS)
            {
                var lineCounter = 0;
                var logLinesSinks = File.ReadAllLines(fileName);
                Assert.Equal(Const.LOG_LINE_COUNT, logLinesSinks.Length);

                foreach (var logLine in logLinesSinks)
                {
                    var actualLineNumber = Helper.GetLineNumber(logLine);
                    Helper.AssertLogLine(lineCounter, actualLineNumber, logLine);
                    lineCounter++;
                }
            }
        }
    }
}

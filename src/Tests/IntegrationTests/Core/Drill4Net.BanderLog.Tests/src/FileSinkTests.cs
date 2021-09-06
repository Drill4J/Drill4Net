using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Drill4Net.Common;
using Drill4Net.BanderLog.Sinks;
using Drill4Net.BanderLog.Sinks.File;
using Helper = Drill4Net.BanderLog.Tests.BanderlogTestsHelper;
using Const = Drill4Net.BanderLog.Tests.BanderlogTestsConstants;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Tests for Banderlog File Sinks
    /// </summary>
    public class FileSinkTests
    {
        private AbstractSink InitializeSink(string fileName)
        {
            return FileSinkCreator.CreateSink(fileName);
        }

        [Fact]
        public void OneThreadOneLoggerTest()
        {
            var logName = Path.GetRandomFileName();
            var filePath = Path.Combine(Const.TEMP_PATH, logName);
            try
            {
                //arrange               
                var logger = InitializeSink(filePath);

                //act
                Helper.WriteLog(logger);
                logger.Shutdown();

                //assert
                var lineCounter = 0;
                var logLines = File.ReadAllLines(filePath);
                Assert.Equal(Const.LOG_LINE_COUNT, logLines.Length);

                foreach (var logLine in logLines)
                {
                    var actualLineNumber = Helper.GetLineNumber(logLine);
                    Helper.AssertLogLine(lineCounter, actualLineNumber, logLine);
                    lineCounter++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonUtils.GetExceptionDescription(ex));
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

        }

        [Fact]
        public void ParallelThreadsOneLoggerTest()
        {
            var logName = Path.GetRandomFileName();
            var filePath = Path.Combine(Const.TEMP_PATH, logName);

            try
            {
                //arrange
                var logger = InitializeSink(filePath);

                //act
                Task[] tasks = new Task[2]
                {
                    new Task(() => Helper.WriteLog(logger, "thread_1_")),
                    new Task(() => Helper.WriteLog(logger, "thread_2_"))
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
                var lineCounterThread1 = 0;
                var lineCounterThread2 = 0;
                var logLinesThreads = File.ReadAllLines(filePath);
                Assert.Equal(Const.LOG_LINE_COUNT * 2, logLinesThreads.Length);

                foreach (var logLine in logLinesThreads)
                {
                    var actualLineNumber = Helper.GetLineNumber(logLine);
                    if (logLine.Contains("thread_1_"))
                    {
                        Helper.AssertLogLine(lineCounterThread1, actualLineNumber, logLine);
                        lineCounterThread1++;
                    }

                    if (logLine.Contains("thread_2_"))
                    {
                        Helper.AssertLogLine(lineCounterThread2, actualLineNumber, logLine);
                        lineCounterThread2++;
                    }
                }

                //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
                Assert.Equal(Const.LOG_LINE_COUNT, lineCounterThread1);
                Assert.Equal(Const.LOG_LINE_COUNT, lineCounterThread2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonUtils.GetExceptionDescription(ex));
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.BanderLog.Sinks;
using Const = Drill4Net.BanderLog.Tests.BanderlogTestsConstants;
using Helper = Drill4Net.BanderLog.Tests.BanderlogTestsHelper;

namespace Drill4Net.BanderLog.Tests
{
    public class FileSinkTests
    {
        public AbstractSink InitializeSink(string fileName)
        {
            return FileSinkCreator.CreateSink(fileName);
        }

        [Fact]
        public void OneThreadOneLoggerTest()
        {
            //arrange
            if(File.Exists(Const.LOG_PATH))
                File.Delete(Const.LOG_PATH);
            var logger = InitializeSink(Const.LOG_PATH);

            //act
            Helper.WriteLog(logger);
            logger.Shutdown();
            Thread.Sleep(10);

            //assert
            var lineCounter = 0;
            var logLines = File.ReadAllLines(Const.LOG_PATH);

            foreach(var logLine in logLines)
            {
                var actualLineNumber = Helper.GetLineNumber(logLine);
                Helper.AssertLogLine(lineCounter, actualLineNumber, logLine);
                lineCounter++;
            }

            //One hundred thousand lines (maybe million) are written to the file and not a single one is lost.
            Assert.Equal(Const.LOG_LINE_COUNT, lineCounter);
        }

        [Fact]
        public void ParallelThreadsOneLoggerTest()
        {
            //arrange
            if(File.Exists(Const.LOG_PATH_THREADS))
                File.Delete(Const.LOG_PATH_THREADS);
            var logger = InitializeSink(Const.LOG_PATH_THREADS);

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
                BanderlogTestsUtils.WriteAggregateException(ae);
            }
            finally
            {
                logger.Shutdown();
                Thread.Sleep(10);
            }
            
            //assert
            var lineCounterThread1 = 0;
            var lineCounterThread2 = 0;
            var logLinesThreads = File.ReadAllLines(Const.LOG_PATH_THREADS);

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
       
    }
}

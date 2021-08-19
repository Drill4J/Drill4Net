using Drill4Net.BanderLog.Sinks;
using System;
using Const = Drill4Net.BanderLog.Tests.BanderlogTestsConstants;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Drill4Net.BanderLog.Tests
{
    static class BanderlogTestsHelper
    {
        public static void WriteLog(ILogSink logger, string additionalInfo = "")
        {
            for (var i = 1; i <= Const.LOG_LINE_COUNT; i++)
            {
                logger.Log(LogLevel.Information, $"{i}_{additionalInfo}{Const.LOG_STRING}");
            }
        }
        public static void AssertLogLine(int expectedLineNumber, int actualLineNumber, string logLine)
        {
            //Check the fact that the lines do not change their order
            //(it's not terrible if it changes within small limits)
            Assert.True(expectedLineNumber <= actualLineNumber && actualLineNumber <= expectedLineNumber + 1);

            //The content is not distorted (at least the last line).
            Assert.EndsWith(Const.LOG_STRING, logLine);
        }
        public static int GetLineNumber(string logLine)
        {
            var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
            Assert.True(int.TryParse(lineNumberInLog, out int actualLineNumber));
            return actualLineNumber;
        }
    }
}

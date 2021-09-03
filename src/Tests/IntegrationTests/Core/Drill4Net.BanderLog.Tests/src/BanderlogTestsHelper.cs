using Drill4Net.BanderLog.Sinks;
using Xunit;
using Microsoft.Extensions.Logging;
using Const = Drill4Net.BanderLog.Tests.BanderlogTestsConstants;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Helper for Banderlog Tests
    /// </summary>
    static class BanderlogTestsHelper
    {
        /// <summary>
        /// Writes data to log with ILogSink
        /// </summary>
        ///<param name="logger">Logger for writing</param>
        ///<param name="additionalInfo">Additional info for log line</param>
        public static void WriteLog(ILogSink logger, string additionalInfo = "")
        {
            for (var i = 1; i <= Const.LOG_LINE_COUNT; i++)
            {
                logger.Log(LogLevel.Information, $"{i}_{additionalInfo}{Const.LOG_STRING}");
            }
        }

        /// <summary>
        /// Get Tree from tree's file
        /// </summary>
        ///<param name="expectedLineNumber">Expected line number</param>
        ///<param name="actualLineNumber">Actual line number</param>
        /// <param name="logLine">Log line for assertion</param>
        public static void AssertLogLine(int expectedLineNumber, int actualLineNumber, string logLine)
        {
            //Check the fact that the lines do not change their order
            //(it's not terrible if it changes within small limits)
            Assert.True(expectedLineNumber <= actualLineNumber && actualLineNumber <= expectedLineNumber + 1);

            //The content is not distorted (at least the last line).
            Assert.EndsWith(Const.LOG_STRING, logLine);
        }

        /// <summary>
        /// Gets line number for log line
        /// </summary>
        ///<param name="logLine">Log line</param>
        /// <returns>Actual line number</returns>
        public static int GetLineNumber(string logLine)
        {
            var lineNumberInLog = logLine.Substring(logLine.LastIndexOf("|") + 1, logLine.IndexOf("_") - logLine.LastIndexOf("|") - 1);
            Assert.True(int.TryParse(lineNumberInLog, out int actualLineNumber));
            return actualLineNumber;
        }
    }
}

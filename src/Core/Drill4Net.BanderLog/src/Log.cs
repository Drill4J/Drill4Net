using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Drill4Net.Configuration;

namespace Drill4Net.BanderLog
{
    public static class Log
    {
        public static ILogger Logger { get; private set; }

        /*************************************************************************/

        public static void Configure(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static void Configure(List<LogData> opts)
        {
            //TODO: use opts!!!
            var logBld = new LogBuilder();
            Logger = logBld.CreateStandardLogger();
        }

        #region Specific
        public static void Trace(string message, Exception exception = null)
        {
            Logger.Log(LogLevel.Trace, message, exception);
        }

        public static void Debug(string message, Exception exception = null)
        {
            Logger.Log(LogLevel.Debug, message, exception);
        }

        public static void Info(string message, Exception exception = null)
        {
            Logger.Log(LogLevel.Information, message, exception);
        }

        public static void Warning(string message, Exception exception = null)
        {
            Logger.Log(LogLevel.Warning, message, exception);
        }

        public static void Error(string message, Exception exception = null)
        {
            Logger.Log(LogLevel.Error, message, exception);
        }

        public static void Fatal(string message, Exception exception = null)
        {
            Logger.Log(LogLevel.Critical, message, exception);
        }
        #endregion
        #region Write
        public static void Write(LogLevel logLevel, string message, Exception exception = null)
        {
            Logger.Log(logLevel, message, exception);
        }

        public static void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Logger.Log(logLevel, eventId, state, exception, formatter);
        }
        #endregion
    }
}

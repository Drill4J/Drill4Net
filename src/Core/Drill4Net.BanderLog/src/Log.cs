using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public static class Log
    {
        public static Logger Logger { get; private set; }

        /*************************************************************************/

        public static void Configure(Logger logger, bool removeOldSinks = false)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (Logger == null || removeOldSinks)
            {
                Logger = logger;
            }
            else
            {
                foreach(var sink in logger.GetSinks())
                    Logger.AddSink(sink);
            }
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

        public static void Shutdown()
        {
            Logger.Shutdown();
        }
    }
}

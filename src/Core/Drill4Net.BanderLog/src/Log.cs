using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public static class Log
    {
        public static Logger Logger { get; private set; }

        /****************************************************************************/

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
        public static void Trace(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Trace, message, exception, callerMethod);
        }

        public static void Debug(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Debug, message, exception, callerMethod);
        }

        public static void Info(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Information, message, exception, callerMethod);
        }

        public static void Warning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Warning, message, exception, callerMethod);
        }

        public static void Error(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Error, message, exception, callerMethod);
        }

        public static void Error(Exception exception, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Error, null, exception, callerMethod);
        }

        public static void Fatal(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(LogLevel.Critical, message, exception, callerMethod);
        }
        #endregion
        #region Write
        public static void Write(LogLevel logLevel, string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Logger?.Log(logLevel, message, exception, callerMethod);
        }

        public static void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Logger?.Log(logLevel, eventId, state, exception, formatter);
        }
        #endregion

        public static void Shutdown()
        {
            Logger?.Shutdown();
        }
    }
}

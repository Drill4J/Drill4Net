using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public static class Log
    {
        public static LogManager Manager { get; private set; }

        /****************************************************************************/

        public static void Configure(LogManager logger, bool removeOldSinks = false)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (Manager == null || removeOldSinks)
            {
                Manager = logger;
            }
            else
            {
                foreach(var sink in logger.GetSinks())
                    Manager.AddSink(sink);
            }
        }

        #region Specific
        #region Trace
        public static void Trace<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(LogLevel.Trace, message, exception, callerMethod);
        }

        internal static void Trace<TState>(ILoggerData loggerData, TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(LogLevel.Trace, loggerData, message, exception, callerMethod, null);
        }
        #endregion
        #region Debug
        public static void Debug<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(LogLevel.Debug, message, exception, callerMethod);
        }

        internal static void Debug<TState>(ILoggerData loggerData, TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(LogLevel.Debug, loggerData, message, exception, callerMethod, null);
        }
        #endregion
        #region Info
        public static void Info<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(LogLevel.Information, message, exception, callerMethod);
        }

        internal static void Info<TState>(ILoggerData loggerData, TState message, Exception exception = null,
            [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(LogLevel.Information, loggerData, message, exception, callerMethod, null);
        }
        #endregion
        #region Warning
        public static void Warning<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(LogLevel.Warning, message, exception, callerMethod);
        }

        internal static void Warning<TState>(ILoggerData loggerData, TState message, Exception exception = null,
            [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(LogLevel.Warning, loggerData, message, exception, callerMethod, null);
        }
        #endregion
        #region Error
        public static void Error<TState>(TState message, Exception exception, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(LogLevel.Error, message, exception, callerMethod);
        }

        public static void Error(Exception exception, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log<string>(LogLevel.Error, null, exception, callerMethod);
        }

        internal static void Error<TState>(ILoggerData loggerData, TState message, Exception exception = null,
            [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(LogLevel.Error, loggerData, message, exception, callerMethod,null);
        }

        internal static void Error(ILoggerData loggerData, Exception exception, [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx<string>(LogLevel.Error, loggerData, null, exception, callerMethod, null);
        }
        #endregion
        #region Fatal
        public static void Fatal<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(LogLevel.Critical, message, exception, callerMethod);
        }

        internal static void Fatal<TState>(ILoggerData loggerData, TState message, Exception exception = null,
            [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(LogLevel.Critical, loggerData, message, exception, callerMethod, null);
        }
        #endregion
        #endregion
        #region Write
        public static void Write<TState>(LogLevel logLevel, TState message, Exception exception = null, 
            [CallerMemberName] string callerMethod = "")
        {
            Manager?.Log(logLevel, message, exception, callerMethod);
        }

        internal static void Write<TState>(LogLevel logLevel, ILoggerData loggerData, TState message, Exception exception = null, 
            [CallerMemberName] string callerMethod = "")
        {
            Manager?.LogEx(logLevel, loggerData, message, exception, callerMethod, null);
        }

        public static void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Manager?.Log(logLevel, eventId, state, exception, formatter);
        }
        #endregion

        public static void Flush()
        {
            Manager?.Flush();
        }

        public static void Shutdown()
        {
            Manager?.Shutdown();
        }
    }
}

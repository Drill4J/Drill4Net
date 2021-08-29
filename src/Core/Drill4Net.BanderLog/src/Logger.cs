using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public class Logger<T> : Logger where T : class
    {
        public Logger() : base(nameof(T))
        {
        }
    }

    public class Logger
    {
        public string Category { get; }

        /***************************************************************************/

        public Logger(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentNullException(nameof(category));
            Category = category;
        }

        /***************************************************************************/

        #region Specific
        public void Trace(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Trace(message, exception, callerMethod);
        }

        public void Debug(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Debug(message, exception, callerMethod);
        }

        public void Info(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Info(message, exception, callerMethod);
        }

        public void Warning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Warning(message, exception, callerMethod);
        }

        public void Error(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Error(message, exception, callerMethod);
        }

        public void Error(Exception exception, [CallerMemberName] string callerMethod = "")
        {
            Log.Error(null, exception, callerMethod);
        }

        public void Fatal(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Fatal(message, exception, callerMethod);
        }
        #endregion
        #region Write
        public static void Write(LogLevel logLevel, string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log.Write(logLevel, message, exception, callerMethod);
        }

        public static void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Log.Write(logLevel, eventId, state, exception, formatter);
        }
        #endregion
    }
}
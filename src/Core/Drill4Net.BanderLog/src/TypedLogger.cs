using System;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Drill4Net.BanderLog
{
    public class TypedLogger<T> : TypedLogger, ILogger<T> where T : class
    {
        public TypedLogger() : base(typeof(T).Name)
        {
        }
    }

    /*******************************************************************************/

    public class TypedLogger : ILogger
    {
        public string Category { get; }

        /***************************************************************************/

        public TypedLogger(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentNullException(nameof(category));
            Category = category;
        }

        /***************************************************************************/

        public LogManager GetSinkManager() => BanderLog.Log.SinkManager;

        #region Specific
        public void Trace(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Trace(message, exception, callerMethod);
        }

        public void Debug(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Debug(message, exception, callerMethod);
        }

        public void Info(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Info(message, exception, callerMethod);
        }

        public void Warning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Warning(message, exception, callerMethod);
        }

        public void Error(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Error(message, exception, callerMethod);
        }

        public void Error(Exception exception, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Error(null, exception, callerMethod);
        }

        public void Fatal(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Fatal(message, exception, callerMethod);
        }
        #endregion
        #region Write
        public static void Write(LogLevel logLevel, string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Write(logLevel, message, exception, callerMethod);
        }

        public static void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            BanderLog.Log.Write(logLevel, eventId, state, exception, formatter);
        }
        #endregion
        #region Interface ILogger
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetSinkManager()?.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return GetSinkManager()?.IsEnabled(logLevel) == true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return GetSinkManager()?.BeginScope(state);
        }
        #endregion

        public override string ToString()
        {
            return Category;
        }
    }
}
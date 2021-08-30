using System;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Drill4Net.BanderLog
{
    public class Logger : ILogger
    {
        public string Subsystem { get; }
        public string Category { get; }

        /***************************************************************************/

        public Logger(string category = null, string subsystem = null)
        {
            Category = category;
            Subsystem = subsystem;
        }

        /***************************************************************************/

        public LogManager GetManager() => BanderLog.Log.Manager;

        #region Specific
        public void Trace(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Trace(Subsystem, Category, message, exception, callerMethod);
        }

        public void Debug(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Debug(Subsystem, Category, message, exception, callerMethod);
        }

        public void Info(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Info(Subsystem, Category, message, exception, callerMethod);
        }

        public void Warning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Warning(Subsystem, Category, message, exception, callerMethod);
        }

        public void Error(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Error(Subsystem, Category, message, exception, callerMethod);
        }

        public void Error(Exception exception, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Error(Subsystem, Category, null, exception, callerMethod);
        }

        public void Fatal(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Fatal(Subsystem, Category, message, exception, callerMethod);
        }
        #endregion
        #region Write
        public void Write(LogLevel logLevel, string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Write(logLevel, Subsystem, Category, message, exception, callerMethod);
        }

        public void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            BanderLog.Log.Write(logLevel, eventId, state, exception, formatter);
        }
        #endregion
        #region Interface ILogger
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetManager()?.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return GetManager()?.IsEnabled(logLevel) == true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return GetManager()?.BeginScope(state);
        }
        #endregion

        public override string ToString()
        {
            return $"{Subsystem}: {Category}";
        }
    }
}
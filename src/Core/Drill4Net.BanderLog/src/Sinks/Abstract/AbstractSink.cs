using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks
{
    public abstract class AbstractSink : ILogSink, IEquatable<AbstractSink>
    {
        #region Log
        public virtual void Log<TState>(LogLevel logLevel, TState message, Exception exception = null, string caller = "")
        {
            Log(logLevel, null, message, exception, caller);
        }

        //TODO: using formatters
        protected virtual void Log<TState>(LogLevel logLevel, ILoggerData loggerData, TState message, Exception exception = null, string caller = "")
        {
            LogEx<TState>(logLevel, loggerData, message, exception, caller, null);
        }

        public abstract void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
        internal abstract void LogEx<TState>(LogLevel logLevel, ILoggerData loggerData, TState state, Exception exception, string caller,
            Func<TState, Exception, string> formatter);
        #endregion

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            //return logLevel == LogLevel.Trace;
            return true;
        }

        public virtual IDisposable BeginScope<TState>(TState state)
        {
            return null; //this?
        }

        public abstract void Flush();

        public abstract void Shutdown();

        public abstract int GetKey();

        public bool Equals(AbstractSink other)
        {
            return GetKey() == other?.GetKey();
        }
    }
}

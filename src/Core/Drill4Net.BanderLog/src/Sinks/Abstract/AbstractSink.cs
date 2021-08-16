using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks
{
    public abstract class AbstractSink : ILogSink
    {
        public virtual void Log(LogLevel logLevel, string message, Exception exception = null)
        {
            Log<string>(logLevel, new EventId(0), message, exception, null);
        }

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            //return logLevel == LogLevel.Trace;
            return true;
        }

        public virtual IDisposable BeginScope<TState>(TState state)
        {
            return null; //this?
        }

        public abstract void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
        public abstract void Flush();

        public abstract void Shutdown();
    }
}

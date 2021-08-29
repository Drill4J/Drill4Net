﻿using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks
{
    public abstract class AbstractSink : ILogSink, IEquatable<AbstractSink>
    {
        //TODO: using formatters
        public virtual void Log(LogLevel logLevel, string message, Exception exception = null, string caller = "")
        {
            if(string.IsNullOrWhiteSpace(caller))
                Log<string>(logLevel, new EventId(0), message, exception, null);
            else
                Log<string>(logLevel, message, exception, caller, null);
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
        public abstract void Log<TState>(LogLevel logLevel, TState state, Exception exception, string caller, Func<TState, Exception, string> formatter);

        public abstract void Flush();

        public abstract void Shutdown();

        public abstract string GetKey();

        public bool Equals(AbstractSink other)
        {
            var key = GetKey();
            if (string.IsNullOrWhiteSpace(key))
                return false;
            return key == other?.GetKey();
        }
    }
}

using Microsoft.Extensions.Logging;
using System;

namespace Drill4Net.BanderLog.Sinks
{
    public interface ILogSink : ILogger
    {
        void Flush();
        void Log(LogLevel logLevel, string state, Exception exception = null);
    }
}
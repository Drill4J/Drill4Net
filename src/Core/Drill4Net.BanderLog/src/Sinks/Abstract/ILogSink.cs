using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks
{
    public interface ILogSink : ILogger
    {
        void Log(LogLevel logLevel, string state, Exception exception = null, string caller = "");

        string GetKey();
        void Flush();
        void Shutdown();
    }
}
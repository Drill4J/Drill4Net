using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.Sinks
{
    public interface ILogSink : ILogger
    {
        Task Flush();
        void Log(LogLevel logLevel, string state, Exception exception = null);
    }
}
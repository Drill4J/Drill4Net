using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks
{
    public interface ILogSink<TСategory> : ILogSink, ILogger<TСategory>
    {
    }

    public interface ILogSink : ILogger
    {
        void Log<TState>(LogLevel logLevel, TState state, Exception exception = null, [CallerMemberName] string caller = "");

        int GetKey();
        void Flush();
        void Shutdown();
    }
}
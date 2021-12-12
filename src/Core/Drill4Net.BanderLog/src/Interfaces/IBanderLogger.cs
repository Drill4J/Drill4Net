using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public interface IBanderLogger : ILoggerData
    {
        bool IsEnabled(LogLevel logLevel);
        IDisposable BeginScope<TState>(TState state);
        ILogManager GetManager();

        void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);

        void Trace<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");
        void Debug<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");
        void Info<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");
        void Warning<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");
        void Error(Exception exception, [CallerMemberName] string callerMethod = "");
        void Error<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");
        void Fatal<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");

        void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
        void Write<TState>(LogLevel logLevel, TState message, Exception exception = null, [CallerMemberName] string callerMethod = "");
    }
}
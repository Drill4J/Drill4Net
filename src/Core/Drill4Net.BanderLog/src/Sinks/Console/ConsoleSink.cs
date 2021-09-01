using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks.Console
{
    public class ConsoleSink : AbstractTextSink
    {
        #region Log
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var data = FormatData(logLevel, eventId, state, exception, formatter);
            System.Console.WriteLine(data);
        }

        internal override void LogEx<TState>(LogLevel logLevel, ILoggerData loggerData, TState state, Exception exception,
            string caller, Func<TState, Exception, string> formatter)
        {
            var data = FormatData(logLevel, caller, state, exception, formatter, loggerData);
            System.Console.WriteLine(data);
        }
        #endregion

        public override void Flush() { }

        public override void Shutdown() { }

        public override string ToString()
        {
            return "Simple console";
        }

        public override int GetKey()
        {
            return "console".GetHashCode();
        }
    }
}

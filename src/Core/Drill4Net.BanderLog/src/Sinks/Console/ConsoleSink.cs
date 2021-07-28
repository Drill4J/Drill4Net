using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks.Console
{
    public class ConsoleSink : AbstractTextSink
    {
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var data = FormatData(logLevel, eventId, state, exception, formatter);
            System.Console.WriteLine(data);
        }

        public override void Flush() { }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;

namespace Drill4Net.BanderLog.Sinks
{
    /// <summary>
    /// Provides formatters for the text log message by specified event parameters
    /// </summary>
    /// <seealso cref="Drill4Net.BanderLog.Sinks.AbstractSink" />
    public abstract class AbstractTextSink : AbstractSink
    {
        private const string DELIM = "|";

        /*************************************************************************************************/

        protected internal string FormatData<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
        {
            //TODO: structure of message - by CONFIG !!!
            var data = $"{CommonUtils.GetPreciseTime()}{DELIM}{logLevel}";

            if(eventId.Id != 0)
                data += $"{DELIM}{eventId}";

            if (formatter != null)
                data += $"{DELIM}{formatter(state, exception)}";
            else
                data += $"{DELIM}{state}";

            return data;
        }

        protected internal string FormatData<TState>(LogLevel logLevel, string caller, TState state, Exception exception,
            Func<TState, Exception, string> formatter, string subsystem, string category, string extra)
        {
            //TODO: structure of message - by CONFIG !!!
            var data = $"{CommonUtils.GetPreciseTime()}{DELIM}{logLevel}";

            if (!string.IsNullOrWhiteSpace(subsystem))
                data += $"{DELIM}{subsystem}";

            if (!string.IsNullOrWhiteSpace(category))
                data += $"{DELIM}{category}";

            if (!string.IsNullOrWhiteSpace(extra))
                data += $"{DELIM}{extra}";

            if (!string.IsNullOrWhiteSpace(caller))
                data += $"{DELIM}{caller}";

            if (formatter != null)
                data += $"{DELIM}{formatter(state, exception)}";
            else
                data += $"{DELIM}{state}";

            return data;
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;

namespace Drill4Net.BanderLog.Sinks
{
    public abstract class AbstractTextSink : AbstractSink
    {
        private const string DELIM = "|";

        /*************************************************************************************************/

        protected internal string FormatData<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
        {
            //TODO: structure of message - by CONFIG !!!
            var data = $"{CommonUtils.GetPreciseTime()}{DELIM}{logLevel}{DELIM}{eventId}{DELIM}{state}";
            if (formatter != null)
                data += $"{DELIM}{formatter(state, exception)}";
            return data;
        }
    }
}

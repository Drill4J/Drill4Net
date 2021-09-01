using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Drill4Net.Common;

namespace Drill4Net.BanderLog.Sinks
{
    /// <summary>
    /// Provides formatters for the text log message by specified event parameters
    /// </summary>
    /// <seealso cref="Drill4Net.BanderLog.Sinks.AbstractSink" />
    public abstract class AbstractTextSink : AbstractSink
    {
        private readonly JsonSerializerSettings _jsonSettings;
        private const string DELIM = "|";

        /*************************************************************************************************/

        protected AbstractTextSink()
        {
            //https://www.newtonsoft.com/json/help/html/SerializationSettings.htm
            _jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };
        }

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
            Func<TState, Exception, string> formatter, ILoggerData loggerData)
        {
            //TODO: structure of message - by CONFIG !!!
            var data = $"{CommonUtils.GetPreciseTime()}{DELIM}{logLevel}";

            if (!string.IsNullOrWhiteSpace(loggerData.Subsystem))
                data += $"{DELIM}{loggerData.Subsystem}";

            if (!string.IsNullOrWhiteSpace(loggerData.Category))
                data += $"{DELIM}{loggerData.Category}";

            if (!string.IsNullOrWhiteSpace(loggerData.ExtrasString))
                data += $"{DELIM}{loggerData.ExtrasString}";

            if (!string.IsNullOrWhiteSpace(caller))
                data += $"{DELIM}{caller}";

            if (formatter != null)
                data += $"{DELIM}{formatter(state, exception)}";
            else
                data += $"{DELIM}{DefaultFormat(state, exception)}";

            return data;
        }

        internal string DefaultFormat<TState>(TState state, Exception ex)
        {
            if (ex == null)
            {
                if (state is string)
                    return state as string;
                return JsonConvert.SerializeObject(state, Formatting.None, _jsonSettings);
            }
            else
            {
                var data = new LogSerializedInfo<TState> { State = state, Exception = ex };
                return JsonConvert.SerializeObject(data, Formatting.None, _jsonSettings);
            }
        }

        private struct LogSerializedInfo<TState>
        {
            internal TState State { get; set; }
            internal Exception Exception { get; set; }
        }
    }
}

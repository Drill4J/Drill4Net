﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog.Sinks;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog
{
    public class Logger : AbstractSink
    {
        public List<ILogSink> Sinks { get; set; }

        /*************************************************************************/

        public Logger(List<ILogSink> sinks = null)
        {
            Sinks = sinks ?? new List<ILogSink>();
        }

        /*************************************************************************/

        public override void Log(LogLevel logLevel, string message, Exception exception = null)
        {
            foreach (var sink in Sinks)
                sink.Log(logLevel, message, exception);
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            foreach (var sink in Sinks)
                sink.Log(logLevel, eventId, state, exception, formatter);
        }

        public override async Task Flush()
        {
            foreach (var sink in Sinks)
                await sink.Flush();
        }
    }
}
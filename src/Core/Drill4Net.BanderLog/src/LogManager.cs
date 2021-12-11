using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Drill4Net.BanderLog.Sinks;

namespace Drill4Net.BanderLog
{
    public class LogManager : AbstractSink, ILogManager
    {
        public ConcurrentDictionary<int, ILogger> Sinks { get; }

        /*****************************************************************************/

        public LogManager(IEnumerable<AbstractSink> sinks = null)
        {
            Sinks = new ConcurrentDictionary<int, ILogger>();
            if (sinks != null)
            {
                foreach (var sink in sinks)
                    AddSink(sink);
            }
        }

        /*****************************************************************************/

        public void AddSink(ILogger sink)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));
            //
            int key = 0;
            switch (sink)
            {
                case AbstractSink abstractSink:
                    key = abstractSink.GetKey();
                    break;
                case ILogger _:
                    key = sink.GetHashCode();
                    break;
            }

            //essentially the same loggers are replaced by the latter
            if (Sinks.ContainsKey(key))
                Sinks.TryRemove(key, out _); //return? exception? remove old? -> add func parameter?
            Sinks.TryAdd(key, sink);
        }

        public IList<ILogSink> GetSinks()
        {
            return Sinks.Values.OfType<ILogSink>().ToList();
        }

        #region Log
        public override void Log<TState>(LogLevel logLevel, TState message, Exception exception = null, [CallerMemberName] string caller = "")
        {
            foreach (var sink in Sinks.Values)
            {
                //sink.Log(logLevel, message, exception, caller,);
                switch (sink)
                {
                    case AbstractSink abstractSink:
                        abstractSink.Log(logLevel, message, exception, caller);
                        break;
                    case ILogger _:
                        sink.Log(logLevel, $"{caller}: {message}", exception, caller);
                        break;
                }
            }
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            foreach (var sink in Sinks.Values)
                sink.Log(logLevel, eventId, state, exception, formatter);
        }

        internal override void LogEx<TState>(LogLevel logLevel, ILoggerData loggerData, TState state, Exception exception,
            string caller, Func<TState, Exception, string> formatter)
        {
            foreach (var sink in Sinks.Values)
            {
                //hmmm....
                switch (sink)
                {
                    case AbstractSink abstractSink:
                        abstractSink.LogEx(logLevel, loggerData, state, exception, caller, formatter);
                        break;
                    case ILogger _:
                        //guanito:
                        var data = new LoggerStateData<TState> { LoggerData = loggerData, State = state };
                        sink.Log<LoggerStateData<TState>>(logLevel, new EventId(0), data, exception, null);
                        break;
                }
            }
        }
        #endregion

        public override void Flush()
        {
            foreach (ILogSink sink in GetSinks())
                sink.Flush();
        }

        public override void Shutdown()
        {
            foreach (ILogSink sink in GetSinks())
                sink.Shutdown();
        }

        public override int GetKey()
        {
            return nameof(LogManager).GetHashCode();
        }
    }

    internal class LoggerStateData<TState>
    {
        internal TState State { get; set; }
        internal ILoggerData LoggerData { get; set; }
    }
}

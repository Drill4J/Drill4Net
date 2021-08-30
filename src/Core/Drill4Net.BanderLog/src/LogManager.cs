using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Drill4Net.BanderLog.Sinks;

namespace Drill4Net.BanderLog
{
    public class LogManager : AbstractSink
    {
        public Dictionary<int, AbstractSink> _sinks;

        /*************************************************************************/

        public LogManager(IEnumerable<AbstractSink> sinks = null)
        {
            _sinks = new Dictionary<int, AbstractSink>();
            if (sinks != null)
            {
                foreach (var sink in sinks)
                    AddSink(sink);
            }
        }

        /*************************************************************************/

        public void AddSink(AbstractSink sink)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));
            //
            var key = sink.GetKey();
            if (_sinks.ContainsKey(key))
                _sinks.Remove(key); //return? exception? remove old? -> add func parameter?
            _sinks.Add(key, sink);
        }

        public IList<AbstractSink> GetSinks()
        {
            return _sinks.Values.ToList();
        }

        #region Log
        public override void Log(LogLevel logLevel, string message, Exception exception = null, [CallerMemberName] string caller = "")
        {
            foreach (var sink in _sinks.Values)
                sink.Log(logLevel, message, exception, caller);
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            foreach (var sink in _sinks.Values)
                sink.Log(logLevel, eventId, state, exception, formatter);
        }

        internal override void LogEx<TState>(LogLevel logLevel, string subsystem, string category, TState state, Exception exception,
            string caller, Func<TState, Exception, string> formatter)
        {
            foreach (var sink in _sinks.Values)
                sink.LogEx(logLevel, subsystem, category, state, exception, caller, formatter);
        }
        #endregion

        public override void Flush()
        {
            foreach (var sink in _sinks.Values)
                sink.Flush();
        }

        public override void Shutdown()
        {
            foreach (var sink in _sinks.Values)
                sink.Shutdown();
        }

        public override int GetKey()
        {
            return "Logger".GetHashCode();
        }
    }
}

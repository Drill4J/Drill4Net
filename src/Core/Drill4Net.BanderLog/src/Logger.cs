using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog.Sinks;
using System.Runtime.CompilerServices;

namespace Drill4Net.BanderLog
{
    public class Logger : AbstractSink
    {
        public Dictionary<string, AbstractSink> _sinks;

        /*************************************************************************/

        public Logger(IEnumerable<AbstractSink> sinks = null)
        {
            _sinks = new Dictionary<string, AbstractSink>();
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

        public override void Log<TState>(LogLevel logLevel, TState state, Exception exception, string caller,
            Func<TState, Exception, string> formatter)
        {
            foreach (var sink in _sinks.Values)
                sink.Log(logLevel, state, exception, caller, formatter);
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            foreach (var sink in _sinks.Values)
                sink.Log(logLevel, eventId, state, exception, formatter);
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

        public override string GetKey()
        {
            return "Logger";
        }


    }
}

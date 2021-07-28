using Drill4Net.BanderLog.Sinks;
using Drill4Net.BanderLog.Sinks.Console;
using Drill4Net.BanderLog.Sinks.File;
using System;

namespace Drill4Net.BanderLog
{
    public class LogBuilder
    {
        private readonly Logger _log;

        /*****************************************************************/

        public LogBuilder()
        {
            _log = new Logger();
        }

        /*****************************************************************/

        public LogBuilder AddSink(ILogSink sink)
        {
            _log.Sinks.Add(sink);
            return this;
        }

        public Logger Build()
        {
            return _log;
        }

        public Logger CreateLogger(LogConfiguration cfg)
        {
            throw new NotImplementedException();
        }

        public Logger CreateStandardLogger(string filepath = null)
        {
            var console = new ConsoleSink();
            _log.Sinks.Add(console);
            //
            var file = new FileSink(filepath);
            _log.Sinks.Add(file);
            //
            return _log;
        }
    }
}

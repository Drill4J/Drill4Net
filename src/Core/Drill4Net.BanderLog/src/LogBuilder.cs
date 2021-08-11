using System;
using Drill4Net.BanderLog.Sinks;
using Drill4Net.BanderLog.Sinks.Console;
using Drill4Net.BanderLog.Sinks.File;
using Microsoft.Extensions.Logging;

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

        public Logger CreateLogger(LogConfiguration cfg) //use LogOptions?
        {
            throw new NotImplementedException();
        }

        public Logger CreateStandardLogger(string filepath = null)
        {
            var console = new ConsoleSink();
            _log.Sinks.Add(console);
            //
            var file = FileSinkCreator.CreateSink(filepath);
            _log.Sinks.Add(file);
            //
            return _log;
        }

        /// <summary>
        /// Creates the standard factory.
        /// Next: ILogger logger = loggerFactory.CreateLogger<Startup>();
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns></returns>
        public ILoggerFactory CreateStandardFactory(string filepath = null)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new ConsoleLoggerProvider());
                builder.AddProvider(new FileLoggerProvider(filepath));
            });
            return loggerFactory;
        }
    }
}

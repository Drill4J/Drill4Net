using System;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog.Sinks;
using Drill4Net.BanderLog.Sinks.Console;
using Drill4Net.BanderLog.Sinks.File;
using Microsoft.Extensions.DependencyInjection;

namespace Drill4Net.BanderLog
{
    public class LogBuilder : ILoggingBuilder
    {
        private readonly SinkManager _log;

        public IServiceCollection Services => throw new NotImplementedException();

        /*****************************************************************/

        public LogBuilder()
        {
            _log = new SinkManager();
        }

        /*****************************************************************/

        public LogBuilder AddSink(AbstractSink sink)
        {
            _log.AddSink(sink);
            return this;
        }

        public SinkManager Build()
        {
            return _log;
        }

        public SinkManager CreateLogger(LogConfiguration cfg) //use LogOptions?
        {
            throw new NotImplementedException();
        }

        public SinkManager CreateStandardLogger(string filepath = null)
        {
            var console = new ConsoleSink();
            _log.AddSink(console);
            //
            var file = FileSinkCreator.CreateSink(filepath);
            _log.AddSink(file);
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

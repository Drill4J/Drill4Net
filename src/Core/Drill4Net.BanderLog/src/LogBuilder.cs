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
        private readonly LogManager _manager;

        public IServiceCollection Services => throw new NotImplementedException();

        /*****************************************************************/

        public LogBuilder()
        {
            _manager = new LogManager();
        }

        /*****************************************************************/

        public LogBuilder AddSink(AbstractSink sink)
        {
            _manager.AddSink(sink);
            return this;
        }

        public LogManager Build()
        {
            return _manager;
        }

        public LogManager CreateLogger(LogConfiguration cfg) //use LogOptions?
        {
            throw new NotImplementedException();
        }

        public LogManager CreateStandardLogger(string filepath = null)
        {
            var console = new ConsoleSink();
            _manager.AddSink(console);
            //
            var file = FileSinkCreator.CreateSink(filepath);
            _manager.AddSink(file);
            //
            return _manager;
        }

        /// <summary>
        /// Creates the standard factory.
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

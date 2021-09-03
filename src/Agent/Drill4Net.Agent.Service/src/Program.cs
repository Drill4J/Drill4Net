using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.BanderLog.Sinks.Console;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //var logBld = new LogBuilder();
           // var logger = logBld.CreateStandardLogger();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ServerHost>();
                    services.AddLogging(ConfigureLogging);
                });
        }

        private static void ConfigureLogging(ILoggingBuilder logBld)
        {
            ////TODO: by cfg!!!
            //var consoleProvider = new ConsoleLoggerProvider();
            //logBld.AddProvider(consoleProvider);

            var filePrvd = new FileLoggerProvider();
            logBld.AddProvider(filePrvd);
        }
    }
}

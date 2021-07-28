using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks.Console
{
    public static class ConsoleLoggerExtensions
    {
        public static ILoggerFactory AddConsole(this ILoggerFactory factory)
        {
            factory.AddProvider(new ConsoleLoggerProvider());
            return factory;
        }
    }
}

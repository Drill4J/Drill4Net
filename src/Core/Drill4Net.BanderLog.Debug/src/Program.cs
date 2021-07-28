using System;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            var logBld = new LogBuilder();

            //var fact = logBld.CreateStandardFactory();
            //var logger = fact.CreateLogger(nameof(Program));

            var logger = logBld.CreateStandardLogger();

            logger.Log(LogLevel.Information, "Started.");

            for(var i=0; i<5; i++)
                logger.Log(LogLevel.Debug, $"Num={i}");

            logger.Flush();

            Console.ReadKey(true);
        }
    }
}

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
using Drill4Net.BanderLog.Sinks.File;

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

            for(var i = 0; i < 5; i++)
                logger.Log(LogLevel.Debug, $"Num={i}");

            logger.Shutdown();
            //
            var filepath = Path.Combine(FileUtils.GetExecutionDir(), "log2.txt");
            var fileLogger = FileSinkCreator.CreateSink(filepath);
            const int cnt = 100000;
            Console.WriteLine($"Write {cnt} records...");
            for (var i = 0; i < cnt; i++)
                fileLogger.LogTrace($"i={i + 1}");
            fileLogger.Shutdown();

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }
    }
}

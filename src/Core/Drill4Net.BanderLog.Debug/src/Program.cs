using System;
using System.IO;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.Common;
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

            for(var i = 0; i < 5; i++)
                logger.Log(LogLevel.Debug, $"Num={i}");

            logger.Shutdown();
            //
            var filepath = Path.Combine(FileUtils.GetExecutionDir(), "log2.txt");
            var fileLogger = FileSinkCreator.CreateSink(filepath);
            var cnt = 100000;
            Console.WriteLine($"Write {cnt} records...");
            for (var i = 0; i < cnt; i++)
                fileLogger.LogTrace($"i={i + 1}");
            fileLogger.Shutdown();

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }
    }
}

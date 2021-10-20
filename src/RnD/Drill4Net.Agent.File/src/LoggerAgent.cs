using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog.Sinks.File;

namespace Drill4Net.Agent.File
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    /// <summary>
    /// Profiler for researches. Now just writes log to the file.
    /// </summary>
    public static class LoggerAgent
    {
        private static readonly FileSink _fileSink;

        /*****************************************************************************/

        static LoggerAgent()
        { 
            var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "crosspoints.txt");
            _fileSink = new FileSink(filepath);
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            _fileSink.Log(LogLevel.Information, data);
            //no slow flush!
        }

        //this method must exists due to common injection's logic
        //TODO: do something with it
        public static void DoCommand(int command, string data)
        {
            _fileSink.Log(LogLevel.Information, $"************ COMMAND: {command} -> {data}");
            _fileSink.Flush();
        }
    }
}

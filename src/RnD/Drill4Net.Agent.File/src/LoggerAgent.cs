using System;
using System.IO;
using System.Reflection;
using Drill4Net.Agent.Abstract;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog.Sinks.File;

namespace Drill4Net.Agent.File
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    /// <summary>
    /// Profiler for researches. Now it just writes log to the file.
    /// </summary>
    public static class LoggerAgent
    {
        public static AgentRepository _rep;
        private static readonly FileSink _fileSink;

        /*****************************************************************************/

        static LoggerAgent()
        {
            var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "crosspoints.txt");
            _fileSink = new FileSink(filepath);
            _rep = new AgentRepository();
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            var ctx = _rep.GetContextId();
            _fileSink.Log(LogLevel.Information, $"[{ctx}] -> {data}");
            //no slow flush!
        }

        //this method must exists due to common injection's logic
        //TODO: do something with it
        public static void DoCommand(int command, string data)
        {
            _fileSink.Log(LogLevel.Information, $"************ COMMAND: [{command}] -> {data}");
            _fileSink.Flush();

            _rep.RegisterCommand(command, data);
        }
    }
}

using System;
using System.IO;
using Drill4Net.Agent.Abstract;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
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

        private const string _debugLogFile = @"d:\drill_log.txt";

        /*****************************************************************************/

        static LoggerAgent()
        {
            var filepath = Path.Combine(FileUtils.EntryDir, "crosspoints.txt");
            //_fileSink = new FileSink(filepath);
            //_rep = new AgentRepository();

            System.IO.File.AppendAllText(_debugLogFile, $"{CommonUtils.GetPreciseTime()}|Log path: {filepath}\n");
        }

        /*****************************************************************************/

        private static readonly object _locker = new();
        public static void RegisterStatic(string data)
        {
            var ctx = _rep?.GetContextId();
            _fileSink?.Log(LogLevel.Information, $"[{ctx}] -> {data}");
            //no slow flush!

            lock (_locker)
            {
                System.IO.File.AppendAllText(_debugLogFile, $"{123}|{data}\n");
            }
        }

        //this method must exists due to common injection's logic
        public static void DoCommand(int command, string data)
        {
            _fileSink?.Log(LogLevel.Information, $"************ COMMAND: [{command}] -> {data}");
            _fileSink?.Flush();

            _rep?.RegisterCommand(command, data);

            lock (_locker)
            {
                System.IO.File.AppendAllText(_debugLogFile, $"{123}|Command [{command}] -> [{data}]\n");
            }
        }
    }
}

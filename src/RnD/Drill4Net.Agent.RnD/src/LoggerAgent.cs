using System;
using System.IO;
using System.Reflection;

namespace Drill4Net.Agent.RnD
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    /// <summary>
    /// Profiler for researches. Now just writes log to the file.
    /// </summary>
    public static class LoggerAgent
    {
        private static readonly string _filepath;
        private static readonly ChannelsQueue _queue;

        /*****************************************************************************/

        static LoggerAgent()
        {
            //D:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.RnD\netstandard2.0\crosspoints.txt - GetCallingAssembly
            //d:\Projects\IHS-bdd.Injected\crosspoints.txt - GetEntryAssembly
            _filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "crosspoints.txt");
            if (File.Exists(_filepath))
                File.Delete(_filepath);

            //we just write probes to the file
            Action<string> action = (string str) => File.AppendAllLines(_filepath, new string[] { str });
            _queue = new ChannelsQueue(action);
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            _queue.Enqueue(data);
        }
    }
}

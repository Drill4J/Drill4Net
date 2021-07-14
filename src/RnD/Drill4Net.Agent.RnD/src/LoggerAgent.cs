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
        private static readonly StreamWriter _writer;

        /*****************************************************************************/

        static LoggerAgent()
        {
            //D:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.RnD\netstandard2.0\crosspoints.txt - GetCallingAssembly
            //d:\Projects\IHS-bdd.Injected\crosspoints.txt - GetEntryAssembly
            _filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "crosspoints.txt");
            if (File.Exists(_filepath))
                File.Delete(_filepath);

            //we just write probes to the file
#pragma warning disable DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
            _writer = File.AppendText(_filepath); //writes to memory and flushes at the end (but perhaps can be leaks & last data losses) - for IHS BDD 09:43 min
            //Action<string> action = (string str) => File.AppendAllLines(_filepath, new string[] { str }); //opens & closes file each time - for IHS BDD 18:08 min
            Action<string> action = (string str) => _writer.WriteLine(str);
            _queue = new ChannelsQueue(action);
#pragma warning restore DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            _queue.Enqueue(data);
        }
    }
}

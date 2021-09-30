using System;
using System.IO;
using System.Reflection;
using Drill4Net.Common;

namespace Drill4Net.Agent.File
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    /// <summary>
    /// Profiler for researches. Now just writes log to the file.
    /// </summary>
    public static class LoggerAgent
    {
        private static readonly string _filepath;
        private static readonly ChannelsQueue<string> _queue;
        private static readonly StreamWriter _writer;

        /*****************************************************************************/

        static LoggerAgent()
        {
            _filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "crosspoints.txt");
            if (System.IO.File.Exists(_filepath))
                System.IO.File.Delete(_filepath);

            //we just write probes to the file
        #pragma warning disable DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
            _writer = System.IO.File.AppendText(_filepath); //writes to memory and flushes at the end (but perhaps can be leaks & last data losses) - for IHS BDD 09:43 min
            //Action<string> action = (string str) => File.AppendAllLines(_filepath, new string[] { str }); //opens & closes file each time - for IHS BDD 18:08 min
            Action<string> action = (string str) => _writer.WriteLine(str);
            _queue = new ChannelsQueue<string>(action);
        #pragma warning restore DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            _queue.Enqueue(data);
        }

        //this method must exists due to common injection's logic
        //TODO: do something with it
        public static void DoCommand(int command, string data)
        {
        }
    }
}

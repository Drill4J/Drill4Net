using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Drill4Net.Agent.RnD
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    public class LoggerAgent
    {
        private static readonly string _filepath;
        //private static readonly object _locker = new object();
        //private static SpinLock sl = new SpinLock();
        private static readonly ChannelsQueue _queue;

        /*****************************************************************************/

        static LoggerAgent()
        {
            //_filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "crosspoints.txt");

            _filepath = @"d:\Projects\IHS-bdd.Injected\crosspoints.txt";
            if (File.Exists(_filepath))
                File.Delete(_filepath);

            Action<string> action = (string str) => File.AppendAllLines(_filepath, new string[] { str });
            _queue = new ChannelsQueue(action);
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            //we just write to the file

            //var ar = data.Split('^'); //data can contains some additional info in the debug mode
            //var probeUid = ar[0];
            //var asmName = ar[1];
            //var funcName = ar[2];
            //var probe = ar[3];

            //bool gotLock = false;
            //try
            //{
            //    sl.Enter(ref gotLock);
            //    File.AppendAllLines(_filepath, new string[] { data });
            //}
            //finally
            //{
            //    if (gotLock)`
            //        sl.Exit();
            //}

            _queue.Enqueue(data);
        }
    }
}

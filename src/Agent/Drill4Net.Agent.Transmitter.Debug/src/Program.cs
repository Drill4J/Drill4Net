using System;
using System.Reflection;
using Drill4Net.Common;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyGitVersion)]

namespace Drill4Net.Agent.Transmitter.Debug
{
    static class Program
    {
        private const ConsoleColor COLOR_INFO = ConsoleColor.Green;
        private const ConsoleColor COLOR_ERROR = ConsoleColor.Red;
        private const ConsoleColor COLOR_DATA = ConsoleColor.Cyan;
        private const ConsoleColor COLOR_DEFAULT = ConsoleColor.White;

        /**********************************************************************************/

        static void Main(string[] args)
        {
            SetTitle();

            //what is loaded into the Target process and used by the Proxy class
            //var trans = DataTransmitter.Transmitter;

            var assemblyFile = "..\\..\\Drill4Net.Agent.Transmitter\\netstandard2.0\\Drill4Net.Agent.Transmitter.dll";
            var assembly = Assembly.LoadFrom(assemblyFile);
            var type = assembly.GetType("Drill4Net.Agent.Transmitter.DataTransmitter");
            var methRegInfo = type.GetMethod("TransmitWithContext");

            //var sender = trans.ProbeSender;

            const string ctx = "DBG";
            WriteMessage($"\nContext: {ctx}");

            while (true)
            {
                WriteMessage("\nInput probe Uid:");
                var input = Console.ReadLine()?.Trim();
                if (input == "q" || input == "Q")
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    input = Guid.NewGuid().ToString();
                WriteMessage($"Uid: {input}", COLOR_DATA);

                //var res = trans.SendProbe(input, ctx); //TODO: return normal Status object

                methRegInfo.Invoke(null, new object[] { input, ctx });

                //Console.WriteLine(res != 0
                //    ? $"Delivered message"
                //    : $"Delivery error: {sender.LastError}");
                //WriteMessage($"Res: {res}", sender.IsError ? COLOR_ERROR : COLOR_INFO);
            }
            Console.ReadKey(true);
        }

        #region Info
        private static void SetTitle()
        {
            var version = GetAppVersion();
            var appName = AppDomain.CurrentDomain.FriendlyName;
            var title = $"{appName} {version}";
            Console.Title = title;
            WriteMessage(title, ConsoleColor.Cyan);
        }

        internal static string GetAppVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
        }

        private static void WriteMessage(string mess, ConsoleColor color = COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = COLOR_DEFAULT;
        }
        #endregion
    }
}

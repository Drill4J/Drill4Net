﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Drill4Net.Common;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyFileVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch)]

[assembly: AssemblyInformationalVersion(
      ThisAssembly.Git.SemVer.Major + "." +
      ThisAssembly.Git.SemVer.Minor + "." +
      ThisAssembly.Git.SemVer.Patch + "-" +
      ThisAssembly.Git.Branch + "+" +
      ThisAssembly.Git.Commit)]

namespace Drill4Net.Agent.Transmitter.Debug
{
    class Program
    {
        private const ConsoleColor COLOR_INFO = ConsoleColor.Green;
        private const ConsoleColor COLOR_ERROR = ConsoleColor.Red;
        private const ConsoleColor COLOR_DATA = ConsoleColor.Cyan;
        private const ConsoleColor COLOR_DEFAULT = ConsoleColor.White;

        /**********************************************************************************/

        static void Main(string[] args)
        {
            //aaa();

            SetTitle();

            //what is loaded into the Target process and used by the Proxy class
            var trans = DataTransmitter.Transmitter;
            var sender = trans.ProbeSender;

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

                var res = trans.SendProbe(input, ctx); //TODO: return normal Status object

                Console.WriteLine(res != 0
                    ? $"Delivered message"
                    : $"Delivery error: {sender.LastError}");
                WriteMessage($"Res: {res}", sender.IsError ? COLOR_ERROR : COLOR_INFO);
            }
            Console.ReadKey(true);
        }

        //private static void aaa()
        //{
        //    StackTrace stackTrace = new StackTrace(1, false);
        //   // var frames = stackTrace.GetFrames();
        //    var type = stackTrace.GetFrame(0).GetMethod().DeclaringType;
        //}

        #region Info
        private static void SetTitle()
        {
            var version = GetAppVersion();
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
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

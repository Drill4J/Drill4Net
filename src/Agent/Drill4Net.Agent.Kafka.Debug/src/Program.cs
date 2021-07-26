using System;
using System.Reflection;
using Drill4Net.Common;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyInformationalVersion(
      ThisAssembly.Git.SemVer.Major + "." +
      ThisAssembly.Git.SemVer.Minor + "." +
      ThisAssembly.Git.SemVer.Patch + "-" +
      ThisAssembly.Git.Branch + "+" +
      ThisAssembly.Git.Commit)]

namespace Drill4Net.Agent.Kafka.Debug
{
    class Program
    {
        private const ConsoleColor COLOR_INFO = ConsoleColor.Green;
        private const ConsoleColor COLOR_ERROR = ConsoleColor.Red;
        private const ConsoleColor COLOR_DATA = ConsoleColor.Cyan;
        private const ConsoleColor COLOR_DEFAULT = ConsoleColor.White;

        /*******************************************************************************/

        static void Main(string[] args)
        {
            SetTitle();

            AbstractRepository<ConverterOptions> rep = new KafkaConsumerRepository();
            IProbeReceiver consumer = new KafkaReceiver(rep);
            var agent = new CoverageAgent(consumer);

            agent.Start();
        }

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

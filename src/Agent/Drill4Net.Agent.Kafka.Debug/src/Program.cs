using System;
using System.Reflection;
using Drill4Net.Common;

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
            IProbeConsumer consumer = new KafkaConsumer(rep);
            var agent = new KafkaAgent(consumer);
            agent.MessageReceived += Agent_MessageReceived;
            agent.ErrorOccured += Agent_ErrorOccured;
            agent.Start();
        }

        private static void Agent_MessageReceived(string message)
        {
            WriteMessage($"Message: {message}");
        }

        private static void Agent_ErrorOccured(bool isFatal, bool isLocal, string error)
        {
            WriteMessage($"Error (Fatal: {isFatal}/Local: {isLocal}): {error}", COLOR_ERROR);
        }

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
    }
}

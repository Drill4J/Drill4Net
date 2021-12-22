// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Configurator.App;
using Drill4Net.BanderLog.Sinks.File;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyGitVersion)]

namespace Drill4Net.Agent.Transmitter.Debug
{
    internal class Program
    {
        private static string _title;
        private static Logger _logger;

        /*********************************************************************/

        private static void Main(string[] args)
        {
            var helper = new ConfiguratorOutputHelper();
            try
            {
                PrepareLogger();
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

                _title = new ConfiguratorInformer(helper).SetTitle();
                _logger.Info($"Start: {_title}");
                var iProc = new InputProcessor(helper);
                _logger.Debug("Starting the input processor...");
                iProc.Start(args);
            }
            catch (Exception ex)
            {
                var err = ex.ToString();
                _logger.Fatal(err);
                helper.WriteMessage(err, ConfiguratorAppConstants.COLOR_ERROR);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.Error($"FirstChanceException:\n{e}");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (_logger != null)
            {
                _logger.Info($"Exit: {_title}");
                Log.Flush();
            }
        }

        /// <summary>
        /// Prepares the logger for the local file log only (without console as automatic output)
        /// </summary>
        /// <returns>The file path to the emergency log</returns>
        public static void PrepareLogger()
        {
            var path = LoggerHelper.GetCommonFilePath(LoggerHelper.LOG_FOLDER);
            var builder = new LogBuilder()
                .AddSink(new FileSink(path))
                .Build();
            Log.Configure(builder);
            _logger = new TypedLogger<Program>(CoreConstants.SUBSYSTEM_CONFIGURATOR);
        }
    }
}
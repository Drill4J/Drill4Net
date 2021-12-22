﻿// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Configurator;
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            var outHelper = new ConfiguratorOutputHelper();
            try
            {
                PrepareLogger();

                _title = new ConfiguratorInformer(outHelper).SetTitle();
                _logger.Info($"Start: {_title}");

                var rep = new ConfiguratorRepository();
                var iProc = new InputProcessor(rep, outHelper);
                _logger.Debug("Starting the input processor...");
                iProc.Start(args);
            }
            catch (Exception ex)
            {
                var err = ex.ToString();
                _logger.Fatal(err);
                outHelper.WriteMessage(err, ConfiguratorAppConstants.COLOR_ERROR);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.Fatal($"FirstChanceException:\n{e}");
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
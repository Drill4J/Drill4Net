using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;
using Drill4Net.BanderLog.Sinks.Console;
using Drill4Net.BanderLog.Sinks;
using Drill4Net.BanderLog.Sinks.File;

namespace Drill4Net.Core.Repository
{
    public abstract class AbstractRepository
    {
        /// <summary>
        /// Prepares the initialize logger (it is usually for simple emergency logging).
        /// </summary>
        public static void PrepareEmergencyLogger(string folder = LoggerHelper.LOG_DIR_DEFAULT)
        {
            var path = LoggerHelper.GetCommonFilePath(folder);
            var logger = new LogBuilder()
                .AddSink(new FileSink(path))
                .Build();
            Log.Configure(logger);
        }
    }

    /******************************************************************************************/


    /// <summary>
    /// Root level of Repository's hieararchy
    /// </summary>
    public abstract class AbstractRepository<TOptions>: AbstractRepository where TOptions : AbstractOptions, new()
    {
        /// <summary>
        /// Gets the name of subsystem.
        /// </summary>
        /// <value>
        /// The subsystem.
        /// </value>
        public string Subsystem { get; }

        /// <summary>
        /// Options for different purposes: injection, communication, testing, etc
        /// </summary>
        public TOptions Options { get; set; }

        /*********************************************************************************/

        protected AbstractRepository(string subsystem)
        {
            Subsystem = subsystem;
        }

        /*********************************************************************************/

        #region Arguments
        public static string GetArgument(string[] args, string parameter, string @default = null)
        {
            var cfgArg = GetArgumentPair(args, parameter);
            return cfgArg?.Contains("=") != true ? @default : cfgArg.Split('=')[1];
        }

        public static string GetArgumentConfigPath(string[] args, string defaultPath = null)
        {
            return GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH, defaultPath);
        }

        internal static string GetArgumentPair(string[] args, string arg)
        {
            return args?.FirstOrDefault(a => a.StartsWith($"-{arg}="));
        }
        #endregion
        #region Log config
        /// <summary>
        /// The preparing the logger. It need be called from the client side.
        /// </summary>
        internal protected void PrepareLogger()
        {
            LogManager logger;
            var bld = new LogBuilder();
            //cfg.MinimumLevel.Verbose(); //global min level must be the most "verbosing"
            if (Options.Logs != null)
            {
                var opts = Options.Logs.Where(a => !a.Disabled).OrderBy(a => a.Level);
                foreach (var opt in opts)
                {
                    AddLogOption(bld, opt);
                }
                logger = bld.Build();
            }
            else
            {
                logger = bld.CreateStandardLogger();
            }
            Log.Configure(logger);
        }

        internal void AddLogOption(LogBuilder bld, LogData logOpt)
        {
            //TODO: set Log level individually for each!!!
            AbstractSink sink;
            switch (logOpt.Type)
            {
                case LogSinkType.Console:
                    sink = new ConsoleSink();
                    break;
                case LogSinkType.File:
                    var path = logOpt.Path ?? LoggerHelper.GetCommonFilePath();
                    if (string.IsNullOrWhiteSpace(Path.GetExtension(path))) //path without file name
                    {
                        var type = Options.Type ?? Subsystem;
                        var fileName = string.IsNullOrWhiteSpace(type) ? LoggerHelper.LOG_FILENAME : $"{type}.log";
                        path = Path.Combine(path, fileName);
                    }
                    path = FileUtils.GetFullPath(path);
                    sink = new FileSink(path);
                    break;
                default:
                    throw new ArgumentException($"Unknown logger sink: {logOpt.Type}");
            }
            bld.AddSink(sink);
        }
        #endregion
    }
}

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Drill4Net.Common
{
    /// <summary>
    /// Root level of Repository's hieararchy
    /// </summary>
    public class AbstractRepository<TOptions> where TOptions : AbstractOptions, new()
    {
        /// <summary>
        /// Gets the name of subsystem.
        /// </summary>
        /// <value>
        /// The subsystem.
        /// </value>
        public string Subsystem { get; }

        /// <summary>
        /// Options for the injection
        /// </summary>
        public TOptions Options { get; set; }

        /*********************************************************************************/

        public AbstractRepository(string subsystem)
        {
            Subsystem = subsystem;
        }

        /*********************************************************************************/

        #region Arguments
        public static string GetArgumentConfigPath(string[] args, string defaultPath = null)
        {
            var cfgArg = GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH);
            return cfgArg == null ? defaultPath : cfgArg.Split('=')[1];
        }

        internal static string GetArgument(string[] args, string arg)
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
            var helper = new LoggerHelper();
            var cfg = new LoggerConfiguration();
            cfg.MinimumLevel.Verbose(); //global min level must be the most "verbosing"
            if (Options.Logs != null)
            {
                var opts = Options.Logs.Where(a => !a.Disabled).OrderBy(a => a.Level);
                foreach (var opt in opts)
                {
                    AddLogOption(cfg, opt, helper);
                }
            }
            Log.Logger = cfg.CreateLogger();
        }

        internal void AddLogOption(LoggerConfiguration cfg, LogOptions logOpt, LoggerHelper helper)
        {
            //https://github.com/serilog/serilog/wiki/Configuration-Basics#overriding-per-sink
            var seriLvl = ConvertToSerilogLogLevel(logOpt.Level);
            switch (logOpt.Type)
            {
                case LogSinkType.Console:
                    cfg.WriteTo.Logger(lc => lc.WriteTo.Console(seriLvl));
                    break;
                case LogSinkType.File:
                    var path = logOpt.Path ?? helper.GetCommonFilePath();
                    if (string.IsNullOrWhiteSpace(Path.GetExtension(path))) //path without file name
                    {
                        var type = Options.Type ?? Subsystem;
                        var fileName = string.IsNullOrWhiteSpace(type) ? LoggerHelper.LOG_FILENAME : $"{type}.log";
                        path = Path.Combine(path, fileName);
                    }
                    path = FileUtils.GetFullPath(path);
                    cfg.WriteTo.Logger(lc => lc.WriteTo.File(path, seriLvl));
                    break;
                default:
                    throw new ArgumentException($"Unknown logger sink: {logOpt.Type}");
            }
        }

        internal LogEventLevel ConvertToSerilogLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => throw new ArgumentException("Need set concrete logging level"),
            };
        }

        /// <summary>
        /// Prepares the initialize logger.
        /// </summary>
        public static void PrepareInitLogger(string folder = LoggerHelper.LOG_DIR_DEFAULT)
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration(folder);
            Log.Logger = cfg.CreateLogger();
        }

        //internal void SetLogLevel(LoggerMinimumLevelConfiguration cfg, LogLevel level)
        //{
        //    switch (level)
        //    {
        //        case LogLevel.Trace: cfg.Verbose(); break;
        //        case LogLevel.Debug: cfg.Debug(); break;
        //        case LogLevel.Information: cfg.Information(); break;
        //        case LogLevel.Warning: cfg.Warning(); break;
        //        case LogLevel.Error: cfg.Error(); break;
        //        case LogLevel.Critical: cfg.Fatal(); break;
        //        case LogLevel.None:
        //            break;
        //    }
        //}
        #endregion
    }
}

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Core.Repository
{
    /// <summary>
    /// Abstract repository for the injection options, retrieving strategy, directories and files, etc
    /// </summary>
    /// <typeparam name="TOptions">Concrete options</typeparam>
    /// <typeparam name="THelper">Helper for manipulating the concrete type of options</typeparam>
    public abstract class AbstractRepository<TOptions, THelper> : BaseRepository
                    where TOptions : BaseOptions, new()
                    where THelper : BaseOptionsHelper<TOptions>, new()
    {
        public string Subsystem { get; }

        /// <summary>
        /// Options for the injection
        /// </summary>
        public TOptions Options { get; set; }

        protected THelper _optHelper;

        /**********************************************************************************/

        protected AbstractRepository(string[] args, string subsystem): this(GetArgumentConfigPath(args), subsystem)
        {
        }

        protected AbstractRepository(string cfgPath, string subsystem)
        {
            Subsystem = subsystem;
            _optHelper = new THelper();

            //options
            if (string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = _optHelper.GetActualConfigPath();
            DefaultCfgPath = cfgPath;
            Options = _optHelper.ReadOptions(cfgPath);

            //logging
            PrepareLogger();
        }

        /**********************************************************************************/

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
        internal void PrepareLogger()
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
        #region Injected Tree
        public virtual InjectedSolution ReadInjectedTree(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                path = FileUtils.GetFullPath(path);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) //search in local dir
            {
                var dir = string.IsNullOrWhiteSpace(Options.TreePath) ? FileUtils.GetExecutionDir() : FileUtils.GetFullPath(Options.TreePath);
                path = Path.Combine(dir, CoreConstants.TREE_FILE_NAME);
            }
            if (!File.Exists(path))
                throw new FileNotFoundException($"Solution Tree not found: [{path}]");
            //
            var bytes2 = File.ReadAllBytes(path);
            using var ms2 = new MemoryStream(bytes2);
            if (_ser.Deserialize(ms2) is not InjectedSolution tree)
                throw new System.Exception($"Tree data not read: [{path}]");
            return tree;
        }

        public virtual string GetTreeFilePath(InjectedSolution tree)
        {
            return GetTreeFilePath(tree.DestinationPath);
        }

        public virtual string GetTreeFilePath(string targetDir)
        {
            if (string.IsNullOrWhiteSpace(targetDir))
                targetDir = FileUtils.GetExecutionDir();
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_NAME);
        }

        public virtual string GetTreeFileHintPath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_HINT_NAME);
        }
        #endregion
    }
}

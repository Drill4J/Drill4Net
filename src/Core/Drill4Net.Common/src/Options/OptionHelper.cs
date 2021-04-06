using System;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Drill4Net.Common
{
    public static class OptionHelper
    {
        private static string DefaultCfgPath { get; set; }

        private static readonly Deserializer _deser;

        /********************************************************************/

        static OptionHelper()
        {
            _deser = new Deserializer();
        }

        /********************************************************************/

        public static MainOptions GenerateOptions(string _defaultCfgPath)
        {
            DefaultCfgPath = _defaultCfgPath;
            return ReadOptions(DefaultCfgPath);
        }

        public static MainOptions ClarifyOptions(string[] args)
        {
            var cfgPath = GetCurrentConfigPath(args);
            var cfg = ReadOptions(cfgPath);
            ClarifySourceDirectory(args, cfg);
            ClarifyDestinationDirectory(args, cfg);
            return cfg;
        }

        public static void NormalizePathes(MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));

            opts.Source.Directory = FileUtils.GetFullPath(opts.Source.Directory);
            opts.Destination.Directory = FileUtils.GetFullPath(opts.Destination.Directory);
            opts.Profiler.Directory = FileUtils.GetFullPath(opts.Profiler.Directory);
            if (opts.Tests?.Directory != null)
                opts.Tests.Directory = FileUtils.GetFullPath(opts.Tests.Directory);
        }

        internal static string GetCurrentConfigPath(string[] args)
        {
            var cfgArg = GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH);
            return cfgArg == null ? DefaultCfgPath : cfgArg.Split('=')[1];
        }

        internal static void ClarifySourceDirectory(string[] args, MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = args?.Length > 1 ? PotentialPath(args[0]) : null;
            if (!string.IsNullOrWhiteSpace(sourceDir))
                opts.Source.Directory = sourceDir;
        }

        internal static void ClarifyDestinationDirectory(string[] args, MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var destDir = args?.Length > 1 ? PotentialPath(args?[1]) : null;
            SetDestinationDirectory(opts, destDir);
        }

        internal static void SetDestinationDirectory(MainOptions opts, string destDir)
        {
            if (string.IsNullOrWhiteSpace(destDir))
                destDir = $"{Path.GetDirectoryName(opts.Source.Directory)}.{opts.Destination.FolderPostfix}";
            opts.Destination.Directory = destDir;
        }

        internal static string PotentialPath(string arg)
        {
            return !arg.StartsWith("-") && (arg.Contains("//") || arg.Contains("\\")) ? arg : null;
        }

        internal static string GetArgument(string[] args, string arg)
        {
            return args?.FirstOrDefault(a => a.StartsWith($"-{arg}="));
        }

        internal static MainOptions ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<MainOptions>(cfg);
            SetDestinationDirectory(opts, null);
            NormalizePathes(opts);
            return opts;
        }

        public static void ValidateOptions(MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = FileUtils.GetFullPath(opts.Source.Directory);
            if (string.IsNullOrEmpty(sourceDir))
                throw new Exception("Source directory name is empty");
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory does not exists: {sourceDir}");
            //
            var destDir = FileUtils.GetFullPath(opts.Destination.Directory);
            if (string.IsNullOrEmpty(destDir))
                throw new Exception("Destination directory name is empty");
        }
    }
}

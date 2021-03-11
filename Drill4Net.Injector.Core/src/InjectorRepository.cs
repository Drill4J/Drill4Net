using System;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Drill4Net.Injector.Core
{
    public class InjectorRepository : IInjectorRepository
    {
        public MainOptions Options { get; set; }

        private readonly string _defCfgPath;
        private readonly Deserializer _deser;

        /************************************************************************/

        public InjectorRepository(): 
            this(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CoreConstants.CONFIG_DEFAULT_NAME))
        {
        }

        public InjectorRepository(string cfgPath)
        {
            _defCfgPath = cfgPath;
            _deser = new Deserializer();
            Options = GenerateOptions();
        }

        public InjectorRepository(string[] args): this()
        {
            Options = ClarifyOptions(args);
        }

        /************************************************************************/

        #region Options
        internal MainOptions GenerateOptions()
        {
            return ReadOptions(_defCfgPath);
        }

        internal MainOptions ClarifyOptions(string[] args)
        {
            var cfgPath = GetCurrentConfigPath(args);
            var cfg = ReadOptions(cfgPath);
            ClarifySourceDirectory(args, cfg);
            ClarifyDestinationDirectory(args, cfg);
            return cfg;
        }

        internal string GetCurrentConfigPath(string[] args)
        {
            var cfgArg = GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH);
            return cfgArg == null ? _defCfgPath : cfgArg.Split('=')[1];
        }

        internal void ClarifySourceDirectory(string[] args, MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = args?.Length > 1 ? PotentialPath(args[0]) : null;
            if (!string.IsNullOrWhiteSpace(sourceDir))
                opts.Source.Directory = sourceDir;
        }

        internal void ClarifyDestinationDirectory(string[] args, MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var destDir = args?.Length > 1 ? PotentialPath(args?[1]) : null;
            SetDestinationDirectory(opts, destDir);
        }

        internal void SetDestinationDirectory(MainOptions opts, string destDir)
        {
            if (string.IsNullOrWhiteSpace(destDir))
                destDir = $"{Path.GetDirectoryName(opts.Source.Directory)}.{opts.Destination.FolderPostfix}";
            opts.Destination.Directory = destDir;
        }

        internal string PotentialPath(string arg)
        {
            return !arg.StartsWith("-") && (arg.Contains("//") || arg.Contains("\\")) ? arg : null;
        }

        internal string GetArgument(string[] args, string arg)
        {
            return args?.FirstOrDefault(a => a.StartsWith($"-{arg}="));
        }

        internal MainOptions ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<MainOptions>(cfg);
            SetDestinationDirectory(opts, null);
            return opts;
        }

        public void ValidateOptions(MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            if (string.IsNullOrEmpty(opts.Source.Directory))
                throw new Exception("Source directory name is empty");
            if (!Directory.Exists(opts.Source.Directory))
                throw new DirectoryNotFoundException("Destination directory does not exists");
            //
            if (string.IsNullOrEmpty(opts.Destination.Directory))
                throw new Exception("Destination directory name is empty");
        }

        public void ValidateOptions()
        {
            ValidateOptions(Options);
        }
        #endregion
    }
}

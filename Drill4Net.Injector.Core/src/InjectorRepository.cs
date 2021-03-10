using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Drill4Net.Injector.Core
{
    public class InjectorRepository : IInjectorRepository
    {
        private readonly string _cfgPath;
        private readonly Deserializer _deser;

        /************************************************************************/

        public InjectorRepository()
        {
            _cfgPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), CoreConstants.CONFIG_NAME);
            _deser = new Deserializer();
        }

        /************************************************************************/

        public MainOptions CreateOptions([NotNull] string[] args)
        {
            var cfg = ReadOptions();

            string sourceDir = string.Empty;
            string destDir = string.Empty;

            if (args.Length > 0)
                sourceDir = args[0];
            if (args.Length > 1 && args[1].Contains("//"))
                sourceDir = args[1];

            if (!string.IsNullOrWhiteSpace(sourceDir))
                cfg.Source.Directory = sourceDir;

            if (string.IsNullOrWhiteSpace(destDir))
                destDir = GetInjectedDirectoryName(cfg.Source.Directory, cfg);
            cfg.Destination.Directory = destDir;

            return cfg;
        }

        internal MainOptions ReadOptions()
        {
            if (!File.Exists(_cfgPath))
                throw new FileNotFoundException($"Options file not found: [{_cfgPath}]");
            var cfg = File.ReadAllText(_cfgPath);
            var opts = _deser.Deserialize<MainOptions>(cfg);
            return opts;
        }

        public string GetInjectedDirectoryName([NotNull] string original, [NotNull] MainOptions opts)
        {
            return $"{Path.GetDirectoryName(original)}.{opts.Destination.FolderPostfix}";
        }

        public void ValidateOptions([NotNull] MainOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Source.Directory))
                throw new Exception("Source directory name is empty");
            if (!Directory.Exists(opts.Source.Directory))
                throw new DirectoryNotFoundException("Destination directory does not exists");
            //
            if (string.IsNullOrEmpty(opts.Destination.Directory))
                throw new Exception("Destination directory name is empty");
        }
    }
}

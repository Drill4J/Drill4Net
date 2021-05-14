using System;
using System.IO;
using System.Linq;
using Drill4Net.Core.Repository;

namespace Drill4Net.Common
{
    public class InjectorOptionsHelper : BaseOptionsHelper<InjectorOptions>
    {
        protected override void PostProcess(InjectorOptions opts) 
        {
            SetDestinationDirectory(opts, null);
            NormalizePathes(opts);
        }

        internal void SetDestinationDirectory(InjectorOptions opts, string destDir)
        {
            if (opts.Destination == null)
                opts.Destination = new DestinationOptions();
            if (string.IsNullOrWhiteSpace(destDir))
            {
                if (opts.Source == null)
                    throw new ArgumentException("Sourse options are empty");
                if (opts.Destination?.FolderPostfix != null)
                    destDir = $"{Path.GetDirectoryName(opts.Source.Directory)}.{opts.Destination.FolderPostfix}";
                else
                    destDir = Path.GetDirectoryName(opts.Source.Directory);
            }

            opts.Destination.Directory = destDir;
        }

        public void NormalizePathes(InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            if (opts.Source?.Directory != null)
                opts.Source.Directory = FileUtils.GetFullPath(opts.Source.Directory);
            if (opts.Destination?.Directory != null)
                opts.Destination.Directory = FileUtils.GetFullPath(opts.Destination.Directory);
            if (opts.Profiler?.Directory != null)
                opts.Profiler.Directory = FileUtils.GetFullPath(opts.Profiler.Directory);
            if (opts.Tests?.Directory != null)
                opts.Tests.Directory = FileUtils.GetFullPath(opts.Tests.Directory);

            //filter for source
            if (opts.Source?.Filter != null)
            {
                NormalizeFilterPathes(opts.Source.Filter.Includes);
                NormalizeFilterPathes(opts.Source.Filter.Excludes);
            }
        }

        internal void NormalizeFilterPathes(SourceFilterParams pars)
        {
            var dirs = pars?.Directories;
            if (dirs == null)
                return;
            for (var i = 0; i < dirs.Count; i++)
                dirs[i] = FileUtils.GetFullPath(dirs[i]);
        }

        internal string GetArgumentConfigPath(string[] args)
        {
            var cfgArg = GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH);
            return cfgArg == null ? DefaultCfgPath : cfgArg.Split('=')[1];
        }

        public InjectorOptions ClarifyOptions(string[] args)
        {
            var cfgPath = GetArgumentConfigPath(args);
            if (string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = GetActualConfigPath();
            var cfg = ReadOptions(cfgPath);
            ClarifySourceDirectory(args, cfg);
            ClarifyDestinationDirectory(args, cfg);
            return cfg;
        }

        internal void ClarifySourceDirectory(string[] args, InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = args?.Length > 1 ? PotentialPath(args[0]) : null;
            if (!string.IsNullOrWhiteSpace(sourceDir))
                opts.Source.Directory = sourceDir;
        }

        internal void ClarifyDestinationDirectory(string[] args, InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var destDir = args?.Length > 1 ? PotentialPath(args?[1]) : null;
            SetDestinationDirectory(opts, destDir);
        }

        internal string PotentialPath(string arg)
        {
            //TODO: regex!!!
            return !arg.StartsWith("-") && (arg.Contains("//") || arg.Contains("\\")) ? arg : null;
        }

        internal string GetArgument(string[] args, string arg)
        {
            return args?.FirstOrDefault(a => a.StartsWith($"-{arg}="));
        }

        public static void ValidateOptions(InjectorOptions opts)
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

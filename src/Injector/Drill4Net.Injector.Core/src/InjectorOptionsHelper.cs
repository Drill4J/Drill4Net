using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Injector.Core
{
    public class InjectorOptionsHelper : BaseOptionsHelper<InjectorOptions>
    {
        protected override void PostProcess(InjectorOptions opts)
        {
            SetDestinationDirectory(opts, null);
            NormalizePaths(opts);
        }

        internal void SetDestinationDirectory(InjectorOptions opts, string destDir)
        {
            if (opts.Destination == null)
                opts.Destination = new DestinationOptions();
            if (string.IsNullOrWhiteSpace(destDir))
            {
                if (string.IsNullOrWhiteSpace(opts.Destination?.Directory))
                    destDir = $"{FileUtils.GetDirectoryName(opts.Source.Directory)}.{opts.Destination?.FolderPostfix ?? "Injected"}";
                else
                    destDir = FileUtils.GetDirectoryName(opts.Destination.Directory);
            }

            opts.Destination.Directory = destDir;
        }

        public void NormalizePaths(InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = opts.Source.Directory;
            opts.Source.Directory = FileUtils.GetFullPath(sourceDir);

            var destDir = opts.Destination.Directory;
            opts.Destination.Directory = FileUtils.GetFullPath(destDir);

            opts.Profiler.Directory = FileUtils.GetFullPath(opts.Profiler.Directory);
            if (opts.Versions?.Directory != null) //extract to separate Test Cfg
                opts.Versions.Directory = FileUtils.GetFullPath(opts.Versions.Directory);

            //filter for source
            var flt = opts.Source?.Filter;
            if (flt != null)
            {
                NormalizeFilterDirs(flt.Includes, sourceDir);
                NormalizeFilterDirs(flt.Excludes, sourceDir);
            }
        }

        internal void NormalizeFilterDirs(SourceFilterParams pars, string basePath)
        {
            var dirs = pars?.Directories;
            if (dirs == null)
                return;
            for (var i = 0; i < dirs.Count; i++)
            {
                var dir = FileUtils.GetFullPath(dirs[i], basePath);
                if (!dir.EndsWith("\\"))
                    dir += dir + "\\";
                dirs[i] = dir;
            }
        }

        #region Clarify        
        /// <summary>
        /// Clarifies some options (such as relative file paths) by additional input
        /// arguments (as a rule, from program command line).
        /// </summary>
        /// <param name="args">The input arguments.</param>
        /// <param name="opts">The current options.</param>
        public void Clarify(CliParser cliParser, InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            if (cliParser == null)
                throw new ArgumentNullException(nameof(cliParser));
            //
            ClarifySourceDirectory(cliParser, opts);
            ClarifyDestinationDirectory(cliParser, opts);
        }

        internal void ClarifySourceDirectory(CliParser cliParser, InjectorOptions opts)
        {
            var sourceDir = cliParser.GetParameter(CoreConstants.ARGUMENT_SOURCE_PATH);
            if (sourceDir == null)
            {
                var aloners = cliParser.GetAloners();
                sourceDir = aloners.Count > 1 ?
                PotentialPath(cliParser.Arguments[0].Value) : //just the first parameter!
                null;
            }
            if (!string.IsNullOrWhiteSpace(sourceDir))
                opts.Source.Directory = FileUtils.GetDirectoryName(sourceDir);
        }

        internal void ClarifyDestinationDirectory(CliParser cliParser, InjectorOptions opts)
        {
            var destDir = cliParser.GetParameter(CoreConstants.ARGUMENT_DESTINATION_PATH);
            if (destDir == null)
            {
                var aloners = cliParser.GetAloners();
                destDir = aloners.Count > 1 ?
                    PotentialPath(cliParser.Arguments[1].Value) : //just the second parameter
                    null;
            }
            if (!string.IsNullOrWhiteSpace(destDir))
                SetDestinationDirectory(opts, destDir);
        }

        //Guanito:
        internal string PotentialPath(string arg)
        {
            //TODO: regex!!!
            return !arg.StartsWith("-") && (arg.Contains("//") || arg.Contains("\\")) ? arg : null;
        }
        #endregion

        public static void ValidateOptions(InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            if (opts.Source == null)
                throw new ArgumentException("Sourсe options are empty");
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

using System;
using System.IO;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Injector.Core
{
    public class InjectorOptionsHelper : BaseOptionsHelper<InjectorOptions>
    {
        protected override void PostProcess(InjectorOptions opts)
        {
            PreliminaryCheckConfig(opts);
            SetDestinationDirectory(opts, null);
            NormalizePaths(opts);
        }

        internal void PreliminaryCheckConfig(InjectorOptions opts)
        {
            //we check this here because Injector can process several configs in specified directory
            if (opts == null || !CoreConstants.SUBSYSTEM_INJECTOR.Equals(opts.Type, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException($"In fact, this is not {CoreConstants.SUBSYSTEM_INJECTOR} options: {opts.Type}");
        }

        public void SetDestinationDirectory(InjectorOptions opts, string destDir)
        {
            if (opts.Destination == null)
                opts.Destination = new DestinationOptions();
            if (string.IsNullOrWhiteSpace(destDir))
            {
                if (string.IsNullOrWhiteSpace(opts.Destination?.Directory))
                    destDir = $"{FileUtils.RefineDirectoryName(opts.Source.Directory)}.{opts.Destination?.FolderPostfix ?? "Injected"}";
                else
                    destDir = FileUtils.RefineDirectoryName(opts.Destination.Directory);
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

            //TODO: extract to separate Test Cfg
            if (opts.Versions?.Directory != null) 
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
        public void Clarify(CliDescriptor cliDescriptor, InjectorOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            if (cliDescriptor == null)
                throw new ArgumentNullException(nameof(cliDescriptor));
            
            // clarify directories
            ClarifySourceDirectory(cliDescriptor, opts);
            ClarifyDestinationDirectory(cliDescriptor, opts);

            //overriding
            if (cliDescriptor != null)
            {
                var silent = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_SILENT);
                if (silent != null)
                    opts.Silent = true;
            }
        }

        internal void ClarifySourceDirectory(CliDescriptor cliDescriptor, InjectorOptions opts)
        {
            var sourceDir = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_SOURCE_PATH);
            if (sourceDir == null)
            {
                var aloners = cliDescriptor.GetPositionals();
                sourceDir = aloners.Count > 1 ?
                PotentialPath(cliDescriptor.Arguments[0].Value) : //just the first parameter!
                null;
            }
            if (!string.IsNullOrWhiteSpace(sourceDir))
                opts.Source.Directory = FileUtils.RefineDirectoryName(sourceDir);
        }

        internal void ClarifyDestinationDirectory(CliDescriptor cliDescriptor, InjectorOptions opts)
        {
            var destDir = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_DESTINATION_PATH);
            if (destDir == null)
            {
                var aloners = cliDescriptor.GetPositionals();
                destDir = aloners.Count > 1 ?
                    PotentialPath(cliDescriptor.Arguments[1].Value) : //just the second parameter
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

        public void ValidateOptions(InjectorOptions opts)
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
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDir}");
            //
            var destDir = FileUtils.GetFullPath(opts.Destination.Directory);
            if (string.IsNullOrEmpty(destDir))
                throw new Exception("Destination directory name is empty");
            if (FileUtils.IsSystemDirectory(destDir, true, out var reason))
                throw new Exception($"Destination directory is bad. {reason}");
            //
            if(FileUtils.AreFoldersNested(sourceDir, destDir))
                throw new Exception("The source and destination directories are nested - this is forbidden.");
            
            //Filter (if no one rule)
            var filter = opts.Source.Filter;
            if(filter.Excludes.GetRuleCount() + filter.Includes.GetRuleCount() == 0)
                throw new Exception("There are no filtering rules for the injected entities.");
        }
    }
}

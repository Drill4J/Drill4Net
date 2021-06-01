﻿using System;
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
            NormalizePaths(opts);
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

        internal string GetArgumentConfigPath(string[] args)
        {
            var cfgArg = GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH);
            return cfgArg == null ? DefaultCfgPath : cfgArg.Split('=')[1];
        }

        /// <summary>
        /// Read options by input argument of program (Injector).
        /// If the arguments do not contain the path to the config, 
        /// the method will try to find the config in other ways
        /// </summary>
        /// <param name="args">Input arguments</param>
        /// <returns></returns>
        public InjectorOptions ReadOptionsFromArgs(string[] args)
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
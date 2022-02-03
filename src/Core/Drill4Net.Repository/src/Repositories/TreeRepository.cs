﻿using System;
using System.IO;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Repository
{
    /// <summary>
    /// Repository that can work with Target's tree <see cref="InjectedSolution"/> from file,
    /// search and check paths for it
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="THelper"></typeparam>
    public abstract class TreeRepository<TOptions, THelper> : ConfiguredRepository<TOptions, THelper>
            where TOptions : TargetOptions, new()
            where THelper : BaseOptionsHelper<TOptions>, new()
    {
        public string TargetNameFromArgs { get; }
        public string TargetVersionFromArgs { get; }

        protected TreeRepositoryHelper _helper;
        private Logger _logger;

        /*****************************************************************************************/

        protected TreeRepository(string subsystem, string cfgPath = null, CliDescriptor cliDescriptor = null) : base(subsystem, cfgPath, cliDescriptor)
        {
            Init();

            if (cliDescriptor != null)
            {
                //overridings from CLI
                TargetNameFromArgs = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_TARGET_NAME);
                if (!string.IsNullOrWhiteSpace(TargetNameFromArgs))
                {
                    _logger.Info($"Target name is overriden: [{Options.Target.Name}] -> [{TargetNameFromArgs}]");
                    Options.Target.Name = TargetNameFromArgs;
                }

                TargetVersionFromArgs = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_TARGET_VERSION);
                var versions = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_TARGET_VERSIONS); //versions by target in Ci pipeline
                if (!string.IsNullOrWhiteSpace(versions))
                {
                    var delim = versions.Contains(",") ? ',' : ';';
                    var ar = versions.Split(delim);

                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(TargetVersionFromArgs))
                    {
                        _logger.Info($"Target version is overriden: [{Options.Target.Version}] -> [{TargetVersionFromArgs}]");
                        Options.Target.Version = TargetVersionFromArgs;
                    }
                }
            }
        }

        protected TreeRepository(string subsystem, TOptions opts) : base(subsystem, opts)
        {
            Init();
        }

        /*****************************************************************************************/

        private void Init()
        {
            _logger = new TypedLogger<TreeRepository<TOptions, THelper>>(Subsystem);
            _helper = new TreeRepositoryHelper(Subsystem);
        }

        #region Injected Tree
        public virtual InjectedSolution ReadInjectedTree(string path = null)
        {
            path = CheckTreeFilePath(path);
            _logger.Debug($"The tree file will be read: [{path}]");

            try
            {
                InjectedSolution tree;
                var bytes2 = File.ReadAllBytes(path);
                try
                {
                    tree = Serializer.FromArray<InjectedSolution>(bytes2);
                }
                catch (Exception ex)
                {
                    throw new System.Exception($"Tree data did't serialized: [{path}].\n{ex}");
                }
                return tree;
            }
            catch (Exception ex)
            {
                throw new Exception($"Can't deserialize Tree data: [{path}]", ex);
            }
        }

        internal string CheckTreeFilePath(string path = null)
        {
            _logger.Debug($"Path param = [{path}]");

            string baseDir;
            if (string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(Options.TreePath))
            {
                baseDir = Options.TreePath;
                _logger.Debug($"Used path from cfg = [{path}]");
            }
            else
                baseDir = FileUtils.EntryDir;

            return _helper.CalculateTreeFilePath(path, baseDir);
        }

        public string GetTreeFilePath(InjectedSolution tree)
        {
            return _helper.GetTreeFilePath(tree);
        }

        public string GetTreeFileHintPath(string path)
        {
            return _helper.GetTreeFileHintPath(path);
        }

        public string GetTreeFilePathByDir(string targetDir)
        {
            return _helper.GetTreeFilePathByDir(targetDir);
        }
        #endregion
    }
}

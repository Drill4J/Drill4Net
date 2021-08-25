using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Core.Repository
{
    /// <summary>
    /// Abstract repository for the injection options, retrieving strategy, directories and files, etc
    /// </summary>
    /// <typeparam name="TOptions">Concrete options</typeparam>
    /// <typeparam name="THelper">Helper for manipulating the concrete type of options</typeparam>
    public abstract class ConfiguredRepository<TOptions, THelper> : AbstractRepository<TOptions>
                    where TOptions : BaseTargetOptions, new()
                    where THelper : BaseOptionsHelper<TOptions>, new()
    {
        /// <summary>
        /// Gets or sets the default config path. Assigned after reading the config file.
        /// </summary>
        /// <value>
        /// The default config path.
        /// </value>
        public string DefaultCfgPath { get; internal set; }

        protected THelper _optHelper;

        /**********************************************************************************/

        protected ConfiguredRepository(string[] args, string subsystem): this(GetArgumentConfigPath(args), subsystem)
        {
        }

        protected ConfiguredRepository(string cfgPath, string subsystem): base(subsystem)
        {
            _optHelper = new THelper();

            //options
            if (string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = _optHelper.GetActualConfigPath(CoreConstants.CONFIG_DEFAULT_NAME);
            DefaultCfgPath = cfgPath;
            Options = _optHelper.ReadOptions(cfgPath);

            //logging
            PrepareLogger();
        }

        protected ConfiguredRepository(TOptions opts, string subsystem) : base(subsystem)
        {
            _optHelper = new THelper();
            Options = opts ?? throw new ArgumentNullException(nameof(opts));
            PrepareLogger();
        }

        /**********************************************************************************/

        #region Injected Tree
        public virtual InjectedSolution ReadInjectedTree(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                path = FileUtils.GetFullPath(path);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) //search in local dir
            {
                var dir = string.IsNullOrWhiteSpace(Options.TreePath) ? FileUtils.GetEntryDir() : FileUtils.GetFullPath(Options.TreePath);
                path = Path.Combine(dir, CoreConstants.TREE_FILE_NAME);
            }
            if (!File.Exists(path))
                throw new FileNotFoundException($"Solution Tree not found: [{path}]");
            //
            try
            {
                InjectedSolution tree;
                var bytes2 = File.ReadAllBytes(path);
                try
                {
                    tree = Serializer.FromArray<InjectedSolution>(bytes2);
                }
                catch(Exception ex)
                {
                    throw new System.Exception($"Tree data not serialized: [{path}].\n{ex}");
                }
                return tree;
            }
            catch (Exception ex)
            {
                throw new Exception($"Can't deserialize Tree data: [{path}]", ex);
            }
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

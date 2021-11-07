using System;
using System.IO;
using Drill4Net.BanderLog;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Core.Repository
{
    public abstract class TreeRepository<TOptions, THelper> : ConfiguredRepository<TOptions, THelper>
            where TOptions : TargetOptions, new()
            where THelper : BaseOptionsHelper<TOptions>, new()
    {
        private Logger _logger;

        /*****************************************************************************************/

        protected TreeRepository(string[] args, string subsystem) : base(subsystem, args)
        {
            CreateTypedLogger();
        }

        protected TreeRepository(string cfgPath, string subsystem) : base(subsystem, cfgPath)
        {
            CreateTypedLogger();
        }

        protected TreeRepository(TOptions opts, string subsystem) : base(subsystem, opts)
        {
            CreateTypedLogger();
        }

        /*****************************************************************************************/

        private void CreateTypedLogger()
        {
            _logger = new TypedLogger<TreeRepository<TOptions, THelper>>(Subsystem);
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

            var optTreeDir = string.IsNullOrWhiteSpace(Options.TreePath) ?
                FileUtils.EntryDir :
                FileUtils.GetFullPath(Options.TreePath);
            //
            if (!string.IsNullOrWhiteSpace(path))
            {
                path = FileUtils.GetFullPath(path);
            }
            else //by hint file
            {
                var hintPath = GetTreeFileHintPath(optTreeDir);
                if (File.Exists(hintPath))
                {
                    _logger.Debug($"The tree hint file founded: [{hintPath}]");
                    path = File.ReadAllText(hintPath);
                }
                else
                {
                    _logger.Debug("The tree hint file not exists");
                }
            }
            //
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) //search in local dir
            {
                path = Path.Combine(optTreeDir, CoreConstants.TREE_FILE_NAME);
                _logger.Debug($"Last chance to get the tree path: [{path}]");
            }
            if (!File.Exists(path))
                throw new FileNotFoundException($"Solution Tree not found: [{path}]");
            //
            return path;
        }

        public virtual string GetTreeFilePath(InjectedSolution tree)
        {
            return GetTreeFilePath(tree.DestinationPath);
        }

        public virtual string GetTreeFilePath(string targetDir)
        {
            if (string.IsNullOrWhiteSpace(targetDir))
                targetDir = FileUtils.ExecutingDir;
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_NAME);
        }

        public virtual string GetTreeFileHintPath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_HINT_NAME);
        }
        #endregion
    }
}

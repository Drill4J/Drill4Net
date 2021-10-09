using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Core.Repository
{
    public abstract class TreeRepository<TOptions, THelper> : ConfiguredRepository<TOptions, THelper>
            where TOptions : TargetOptions, new()
            where THelper : BaseOptionsHelper<TOptions>, new()
    {
        protected TreeRepository(string[] args, string subsystem) : base(args, subsystem)
        {
        }

        protected TreeRepository(string cfgPath, string subsystem) : base(cfgPath, subsystem)
        {
        }

        protected TreeRepository(TOptions opts, string subsystem) : base(opts, subsystem)
        {
        }

        /**********************************************************************************/

        #region Injected Tree
        public virtual InjectedSolution ReadInjectedTree(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                path = FileUtils.GetFullPath(path);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) //search in local dir
            {
                var dir = string.IsNullOrWhiteSpace(Options.TreePath) ? FileUtils.EntryDir : FileUtils.GetFullPath(Options.TreePath);
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

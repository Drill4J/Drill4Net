using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Core.Repository
{
    public abstract class AbstractRepository<TOptions, THelper> where TOptions : BaseOptions, new()
                                                                where THelper : BaseOptionsHelper<TOptions>, new()
    {
        public TOptions Options { get; set; }

        protected THelper _optHelper;

        /**********************************************************************************/

        protected AbstractRepository(string cfgPath)// : this()
        {
            _optHelper = new THelper();
            if (string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = _optHelper.GetActualConfigPath();
            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"Config not found: {cfgPath}");
            Options = _optHelper.GetOptions(cfgPath);
        }

        /**********************************************************************************/

        #region Injected Tree
        public virtual InjectedSolution ReadInjectedTree(string path = null)
        {
            var types = InjectedSolution.GetInjectedTreeTypes();
            var ser = new NetSerializer.Serializer(types);

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) //search in local dir
            {
                var dir = string.IsNullOrWhiteSpace(Options.TreePath) ? FileUtils.GetEntryDir() : FileUtils.GetFullPath(Options.TreePath);
                path = Path.Combine(dir, CoreConstants.TREE_FILE_NAME);
            }
            if (!File.Exists(path))
                throw new FileNotFoundException($"Solution Tree not found: [{path}]");

            var bytes2 = File.ReadAllBytes(path);
            using var ms2 = new MemoryStream(bytes2);
            var tree = ser.Deserialize(ms2) as InjectedSolution;
            return tree;
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

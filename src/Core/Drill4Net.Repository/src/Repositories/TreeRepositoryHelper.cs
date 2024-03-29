﻿using System.IO;
using Drill4Net.BanderLog;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Repository
{
    public class TreeRepositoryHelper
    {
        private readonly Logger _logger;

        /****************************************************************************/

        public TreeRepositoryHelper(string subsystem)
        {
            _logger = new TypedLogger<TreeRepositoryHelper>(subsystem);
        }

        /****************************************************************************/

        public string CalculateTreeFilePath(string baseDir = null)
        {
            return CalculateTreeFilePath(null, baseDir ?? FileUtils.EntryDir);
        }

        internal string CalculateTreeFilePath(string path, string baseDir)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                path = FileUtils.GetFullPath(path);
            }
            else //by hint file
            {
                var hintPath = GetTreeFileHintPath(baseDir);
                if (File.Exists(hintPath))
                {
                    _logger.Debug($"The tree hint file found: [{hintPath}]");
                    path = File.ReadAllText(hintPath);
                }
                else
                {
                    _logger.Debug("The tree hint file not exists");
                }
            }
            //
            path = FileUtils.GetFullPath(path);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) //search in local dir
            {
                path = Path.Combine(baseDir, CoreConstants.TREE_FILE_NAME);
                _logger.Debug($"Last chance to get the tree path: [{path}]");
            }
            path = FileUtils.GetFullPath(path);
            if (!File.Exists(path))
            {
                var trgDir = Path.GetDirectoryName(path);
                if (!Directory.Exists(trgDir))
                    throw new FileNotFoundException($"Target directory not found: [{trgDir}]");
                throw new FileNotFoundException($"Solution Tree not found: [{path}]");
            }
            return path;
        }

        public string GetTreeFilePath(InjectedSolution tree)
        {
            return GetTreeFilePathByDir(tree.DestinationPath);
        }

        public string GetTreeFilePathByDir(string targetDir)
        {
            if (string.IsNullOrWhiteSpace(targetDir))
                targetDir = FileUtils.ExecutingDir;
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_NAME);
        }

        public string GetTreeFileHintPath(string targetDir)
        {
            return FileUtils.GetFullPath(Path.Combine(targetDir, CoreConstants.TREE_FILE_HINT_NAME));
        }
    }
}

﻿using System;
using System.IO;
using System.Linq;
using Serilog;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class TreeDeployer
    {
        private readonly IInjectorRepository _rep;

        /**************************************************************/

        public TreeDeployer(IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
        }

        /**************************************************************/

        public void InjectTree(InjectedSolution tree)
        {
            SaveTree(tree);
            NotifyAboutTree(tree);
        }

        internal void SaveTree(InjectedSolution tree)
        {
            var path = _rep.GetTreeFilePath(tree);
            _rep.WriteInjectedTree(path, tree);
        }

        internal void NotifyAboutTree(InjectedSolution tree)
        {
            //in each folder create file with path to tree data
            var dirs = tree.GetAllDirectories().ToList();
            if (!dirs.Any())
                return;
            var pathInText = _rep.GetTreeFilePath(tree);
            Log.Debug($"Tree saved to: [{pathInText}]");
            foreach (var dir in dirs)
            {
                var hintPath = _rep.GetTreeFileHintPath(dir.DestinationPath);
                File.WriteAllText(hintPath, pathInText);
                Log.Debug($"Hint placed to: [{hintPath}]");
            }
        }
    }
}
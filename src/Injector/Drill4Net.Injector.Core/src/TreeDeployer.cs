using System;
using System.IO;
using System.Linq;
using Drill4Net.BanderLog;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Deployer for the Tree data
    /// </summary>
    public class TreeDeployer
    {
        private readonly IInjectorRepository _rep;
        private readonly Logger _logger;

        /**************************************************************/

        public TreeDeployer(IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<AssemblyReader>(rep.Subsystem);
        }

        /**************************************************************/

        public void Deploy(InjectedSolution tree)
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
            Console.WriteLine("");
            _logger.Debug($"Tree saved to: [{pathInText}]");

            foreach (var dir in dirs)
            {
                var hintPath = _rep.GetTreeFileHintPath(dir.DestinationPath);
                File.WriteAllText(hintPath, pathInText);
                _logger.Debug($"Hint placed to: [{hintPath}]");
            }
        }
    }
}

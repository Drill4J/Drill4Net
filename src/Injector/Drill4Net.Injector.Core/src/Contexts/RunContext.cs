using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Context for a separate single run of the Injector on a certain folder
    /// </summary>
    public class RunContext
    {
        public InjectedSolution Tree { get; }
        public string RootDirectory => Tree.Path;
        public string SourceDirectory { get; set; }
        public string SourceFile { get; set; }
        public InjectorOptions Options => Repository.Options;

        public bool? IsNetCore { get; private set; }
        public AssemblyVersioning MainVersion { get; private set; }
        public Dictionary<string, string> Paths { get; private set; }
        public Dictionary<string, AssemblyVersioning> Versions { get; }

        public readonly IInjectorRepository Repository;

        /***************************************************************************************/

        public RunContext(IInjectorRepository rep, InjectedSolution tree)
        {
            Repository = rep ?? throw new ArgumentNullException(nameof(rep));
            Tree = tree ?? throw new ArgumentNullException(nameof(tree));
            Paths = new Dictionary<string, string>();
            Versions = DefineTargetVersions(RootDirectory);
        }

        /***************************************************************************************/

        internal Dictionary<string, AssemblyVersioning> DefineTargetVersions(string rootDir)
        {
            if (!Directory.Exists(rootDir))
                throw new DirectoryNotFoundException($"Source directory not exists: [{rootDir}]");
            var files = Repository.GetAssemblies(rootDir);
            var versions = new Dictionary<string, AssemblyVersioning>();

            //'exe' must be after 'dll'
            foreach (var file in files.OrderBy(a => a))
            {
                AssemblyVersioning version;
                if (IsNetCore == true && Path.GetExtension(file) == ".exe")
                {
                    var dll = Path.Combine(Path.ChangeExtension(file, ".dll"));
                    var dllVer = versions.FirstOrDefault(a => a.Key == dll).Value;
                    version = dllVer ?? new AssemblyVersioning() { Target = AssemblyVersionType.NetCore };
                    MainVersion = version;
                    versions.Add(file, version);
                    continue;
                }
                version = Repository.GetAssemblyVersion(file);
                versions.Add(file, version);
                //
                if (IsNetCore == null)
                {
                    switch (version.Target)
                    {
                        case AssemblyVersionType.NetCore:
                            MainVersion = version;
                            IsNetCore = true;
                            break;
                        case AssemblyVersionType.NetFramework:
                            MainVersion = version;
                            IsNetCore = false;
                            break;
                    }
                }
            }
            return versions;
        }
    }
}

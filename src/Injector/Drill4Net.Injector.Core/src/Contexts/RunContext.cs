using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Context for a separate single run of the Injector on a certain folder
    /// </summary>
    public class RunContext : IDisposable
    {
        /// <summary>
        /// The metadata of the injected Target projects (directories, assemblies, 
        /// classes, methods, cross-points, etc)
        /// </summary>
        public InjectedSolution Tree { get; }

        /// <summary>
        /// Root directory of Injector Engine's Run
        /// </summary>
        public string RootDirectory => Tree.Path;

        /// <summary>
        /// Current source directory during processing
        /// </summary>
        public string ProcessingDirectory { get; set; }

        /// <summary>
        /// Current source file path during processing
        /// </summary>
        public string ProcessingFile { get; set; }

        /// <summary>
        /// Target frameworks (monikers) from options which needed for processing
        /// </summary>
        public List<string> Monikers { get; set; }

        public List<string> MonikerDirectories { get; }

        /// <summary>
        /// Options for injecting process
        /// </summary>
        public InjectorOptions Options => Repository.Options;

        /// <summary>
        /// The abstract member of the injection strategy 
        /// </summary>
        public CodeHandlerStrategy Strategy { get; }

        /// <summary>
        /// Interface of assembly injector
        /// </summary>
        public IAssemblyInjector Injector { get; }

        /// <summary>
        /// Does the current project has Net Core type?
        /// </summary>
        public bool? IsNetCore { get; private set; }

        /// <summary>
        /// Version of framework with some metadata of the current project/service/frontend
        /// </summary>
        public AssemblyVersioning FrameworkVersion { get; private set; }

        /// <summary>
        /// Outcome paths of processed assemblies by Guid of the corresponding assembly context 
        /// </summary>
        public Dictionary<string, string> AssemblyPaths { get; }

        /// <summary>
        /// Versions of processed assemblies by path to file 
        /// </summary>
        public Dictionary<string, AssemblyVersioning> Versions { get; }

        /// <summary>
        /// The repository of the Injector Engine
        /// </summary>
        public IInjectorRepository Repository { get; }

        /// <summary>
        /// IL code generator of the injected Profiler's type
        /// </summary>
        public IProfilerProxyInjector ProxyGenerator { get; }

        /// <summary>
        /// Generated proxy namespaces by assembly Key
        /// </summary>
        public Dictionary<string, string> ProxyNamespaceByKeys { get; set; }

        /***************************************************************************************/

        /// <summary>
        /// Create the context for a separate single run of the Injector on a certain folder
        /// </summary>
        public RunContext(IInjectorRepository rep, InjectedSolution tree)
        {
            Repository = rep ?? throw new ArgumentNullException(nameof(rep));
            Tree = tree ?? throw new ArgumentNullException(nameof(tree));
            AssemblyPaths = new Dictionary<string, string>();
            MonikerDirectories = new List<string>();
            Monikers = new List<string>();
            ProxyNamespaceByKeys = new Dictionary<string, string>();
            Versions = DefineProjectVersions(RootDirectory);
            Strategy = Repository.GetStrategy();
            Injector = Repository.GetInjector();
            ProxyGenerator = Repository.GetProxyGenerator();
        }

        /***************************************************************************************/

        /// <summary>
        /// Define target project's assembly versions for the current moniker (version framework)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal Dictionary<string, AssemblyVersioning> DefineProjectVersions(string dir)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Source directory not exists: [{dir}]");
            var files = Repository.GetAssemblies(dir);
            var versions = new Dictionary<string, AssemblyVersioning>();
            var filter = Options.Source.Filter;

            //we should be located in the project folder of a specific moniker,
            //not in root dir for the project or whole Injector Engine Run

            //as hint 'exe' must be first, after 'dll'
            foreach (var filePath in files.OrderBy(a => a))
            {
                if (!filter.IsFileNeedByPath(filePath))
                    continue;

                AssemblyVersioning version;
                if (IsNetCore == true && Path.GetExtension(filePath) == ".exe")
                {
                    var dll = Path.Combine(Path.ChangeExtension(filePath, ".dll"));
                    var dllVer = versions.FirstOrDefault(a => a.Key == dll).Value;
                    version = dllVer ?? new AssemblyVersioning() { FrameworkType = AssemblyVersionType.NetCore };
                    FrameworkVersion = version;
                    versions.Add(filePath, version);
                    continue;
                }

                version = Repository.TryGetAssemblyVersion(filePath);
                versions.Add(filePath, version);

                if (IsNetCore == null && version != null) //no data yet
                {
                    switch (version.FrameworkType)
                    {
                        case AssemblyVersionType.NetCore:
                            FrameworkVersion = version;
                            IsNetCore = true;
                            break;
                        case AssemblyVersionType.NetFramework:
                            FrameworkVersion = version;
                            IsNetCore = false;
                            break;
                    }
                }
            }
            return versions;
        }

        /// <summary>
        /// Inject the assembly by it's context
        /// </summary>
        /// <param name="asmCtx"></param>
        public Task Inject(AssemblyContext asmCtx)
        {
            return Injector.Inject(this, asmCtx);
        }

        public void Dispose() //TODO: full Dispose pattern
        {
            ProxyGenerator.Dispose();
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Plugins.SpecFlow;
using Drill4Net.Injector.Plugins.SpecFlow;

/*** INFO
     automatic version tagger including Git info - https://github.com/devlooped/GitInfo
     semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
     the most common format is v0.0 (or just 0.0 is enough)
     to change semVer it is nesseccary to create appropriate tag and push it to remote repository
     patches'(commits) count starts with 0 again after new tag pushing
     For file version format exactly is digit
***/
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyGitVersion)]

[assembly: InternalsVisibleToAttribute("Drill4Net.Injector.Engine.UnitTests")]
namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// High-level Injector Engine working with target directories and files. 
    /// It injects the Agent's proxy code to the needed methods and produces 
    /// the Tree metadata of processed entities (directories, assemblies, 
    /// classes, methods, cross-points, etc)
    /// </summary>
    public class InjectorEngine : IInjectorEngine
    {
        /* INFO *
            http://ilgenerator.apphb.com/ - online C# -> IL
            https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C# (a bit buggy and with some restrictions)
                on Github - https://github.com/adrianoc/cecilifier
            https://stackoverflow.com/questions/2508828/where-to-learn-about-vs-debugger-magic-names/2509524#2509524 - naming of CG-entites
            https://www.codeproject.com/Articles/671259/Reweaving-IL-code-with-Mono-Cecil
            https://blog.elishalom.com/2012/02/04/monitoring-execution-using-mono-cecil/
            https://stackoverflow.com/questions/48090703/run-mono-cecil-in-net-core

            Mono.Cecil maintainer: jbevain@gmail.com
        */

        /// <summary>
        /// Plugins for additional injections into assemblies
        /// </summary>
        public List<IInjectorPlugin> Plugins { get; }

        private readonly Logger _logger;
        private readonly IInjectorRepository _rep;

        /*****************************************************************/

        /// <summary>
        /// Create the Injector Engine working with target directories and files
        /// </summary>
        public InjectorEngine(IInjectorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<InjectorEngine>(rep.Subsystem);
            Plugins = GetPlugins(rep.Options);
        }

        /*****************************************************************/

        #region Plugins
        private List<IInjectorPlugin> GetPlugins(InjectorOptions opts)
        {
            var plugins = new List<IInjectorPlugin>();

            //TODO: loads them dynamically from the disk by cfg

            // standard plugins //

            //now we work with already injected Destination, not Source
            var dir = opts.Destination.Directory;
            var proxyClass = opts.Proxy.Class;
            //...and use generated Proxy namespace

            //SpecFlow
            var plugCfg = GetPluginOptions(SpecFlowGeneratorContexter.PluginName, opts.Plugins);
            if (!string.IsNullOrWhiteSpace(plugCfg?.Directory))
                plugins.Add(new SpecFlowHookInjector(dir, proxyClass, plugCfg));

            return plugins;
        }

        internal PluginLoaderOptions GetPluginOptions(string name, Dictionary<string, PluginLoaderOptions> cfgPlugins)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (cfgPlugins?.ContainsKey(name) != true)
                return null; //maybe it is normal for some plugins
            //
            var opts = cfgPlugins[name];

            //some opt's preproc
            opts.Directory = FileUtils.GetFullPath(opts.Directory);

            return opts;
        }

        /// <summary>
        /// Injecting some features by extending plugins
        /// </summary>
        /// <param name="runCtx"></param>
        private void ProcessByPlugins(RunContext runCtx)
        {
            if (Plugins.Count == 0)
                return;
            //
            runCtx.AssemblyPaths.Clear();
            Console.WriteLine("");
            _logger.Info("Processing by plugins...");
            foreach (var plugin in Plugins)
            {
                _logger.Info($"Processing by [{plugin.Name}]");
                plugin?.Process(runCtx);
            }
        }
        #endregion

        /// <summary>
        /// Inject the target accordingly by the current config from repository
        /// </summary>
        /// <returns>Tree of metadata for the injected entities (processed directories, assemblies,
        /// classes, methods, cross-points, etc)</returns>
        public Task<InjectedSolution> Process()
        {
            return Process(_rep.Options);
        }

        /// <summary>
        ///  Inject the target accordingly by the config form parameters 
        /// </summary>
        /// <param name="opts">Config for target's injection</param>
        /// <returns>Tree data of the injection (processed directories, assemblies, classes, methods, cross-points, and their meta-data)</returns>
        public async Task<InjectedSolution> Process(InjectorOptions opts)
        {
            _logger.Debug("Process is starting...");
            InjectorOptionsHelper.ValidateOptions(opts);

            await CopySource(opts)
                .ConfigureAwait(false);

            using var runCtx = CreateRunContext(opts);

            await InjectSource(runCtx)
                .ConfigureAwait(false);

            var tree = runCtx.Tree;
            tree.ProductVersion = opts.Target.Version ?? tree.SearchProductVersion(opts.Target.VersionAssemblyName);
            DeployInjectedTree(tree);

            ProcessByPlugins(runCtx);

            #region Debug
            // debug TODO: to tests
            //var methods = tree.GetAllMethods().ToList();
            //var cgMeths = methods.Where(a => a.IsCompilerGenerated).ToList();
            //var emptyCGInfoMeths = cgMeths
            //    .Where(a => a.CGInfo == null)
            //    .ToList();
            //var emptyBusinessMeths = cgMeths
            //    .Where(a => a.CGInfo!= null && a.CGInfo.Caller != null && (a.BusinessMethod == null || a.BusinessMethod == a.FullName))
            //    .ToList();
            //var nonBlockings = cgMeths.FirstOrDefault(a => a.FullName == "System.String Drill4Net.Target.Common.InjectTarget/<>c::<Async_Linq_NonBlocking>b__54_0(Drill4Net.Target.Common.GenStr)");
            //
            //var points = tree.GetAllPoints().ToList();
            #endregion
            return tree;
        }

        /// <summary>
        /// Copying of all needed data in needed targets
        /// </summary>
        /// <param name="opts"></param>
        internal async Task CopySource(InjectorOptions opts)
        {
            var sourceDir = opts.Source.Directory;
            var destDir = opts.Destination.Directory;
            var monikers = opts.Versions?.Targets;

            _logger.Debug($"The source is copying: [{sourceDir}] to [{destDir}]");
            await _rep.CopySource(sourceDir, destDir, monikers) //TODO: copy dirs only according to the monikers
                .ConfigureAwait(false);
            _logger.Info("The source is copied");
        }

        internal RunContext CreateRunContext(InjectorOptions opts)
        {
            var sourceDir = opts.Source.Directory;
            var destDir = opts.Destination.Directory;

            var tree = new InjectedSolution(opts.Target?.Name, sourceDir)
            {
                StartTime = DateTime.Now,
                DestinationPath = destDir,
                Description = opts.Description,
            };

            return new RunContext(_rep, tree);
        }

        internal async Task InjectSource(RunContext runCtx)
        {
            var sourceDir = runCtx.ProcessingDirectory ?? runCtx.RootDirectory;
            var monikers = runCtx.Options.Versions?.Targets;
            var isMonikersRoot = monikers?.Count > 0 && sourceDir == runCtx.RootDirectory;
            runCtx.Monikers = monikers?.Keys == null ? new List<string>() : monikers?.Keys?.ToList();

            //NO PARALLEL EXECUTION !

            //inner folders: possible targets from cfg
            var dirs = Directory.GetDirectories(sourceDir, "*");
            foreach (var dir in dirs)
            {
                if (!InjectorCoreUtils.IsDirectoryNeedByMoniker(monikers, sourceDir, dir))
                    continue;
                //
                if(isMonikersRoot)
                    runCtx.MonikerDirectories.Add(dir);
                runCtx.ProcessingDirectory = dir;
                await ProcessDirectory(runCtx).ConfigureAwait(false);
            }

            //possible files in the root directly
            if (!runCtx.Tree.GetAllAssemblies().Any()) //get only the needed assemblies
            {
                runCtx.ProcessingDirectory = runCtx.RootDirectory;
                await ProcessDirectory(runCtx).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Process directory from current Engine's context
        /// </summary>
        /// <param name="runCtx">Context of Engine's Run</param>
        /// <returns>Is the directory processed?</returns>
        internal async Task<bool> ProcessDirectory(RunContext runCtx)
        {
            var opts = runCtx.Options;
            var directory = runCtx.ProcessingDirectory;
            if(!InjectorCoreUtils.IsNeedProcessDirectory(opts.Source.Filter, directory, directory == runCtx.RootDirectory))
                return false;
            _logger.Info($"Processing dir [{directory}]");

            //NO PARALLEL EXECUTION !

            //files
            var files = _rep.GetAssemblies(directory);
            foreach (var file in files)
            {
                runCtx.ProcessingFile = file;
                await ProcessFile(runCtx).ConfigureAwait(false);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*");
            foreach (var dir in dirs)
            {
                runCtx.ProcessingDirectory = dir;
                await ProcessDirectory(runCtx).ConfigureAwait(false);
            }
            return true;
        }

        /// <summary>
        /// Process file from current Engine's context
        /// </summary>
        /// <param name="runCtx">Context of Engine's Run</param>
        ///<returns>Is the file processed?</returns>
        private async Task<bool> ProcessFile(RunContext runCtx)
        {
            #region Checks
            var opts = runCtx.Options;
            var filePath = runCtx.ProcessingFile;

            //filter
            if(!InjectorCoreUtils.IsNeedProcessFile(opts.Source.Filter, filePath))
                return false;
            #endregion

            try
            {
                //reading
                var reader = new AssemblyReader();
                using var asmCtx = reader.ReadAssembly(runCtx);
                if (asmCtx.Skipped)
                    return false;

                if (!Directory.Exists(asmCtx.DestinationDir))
                    Directory.CreateDirectory(asmCtx.DestinationDir);

                //processing
                await runCtx.Inject(asmCtx).ConfigureAwait(false);

                _logger.Debug($"Injected: [{filePath}]");

                //writing modified assembly and symbols to new file
                var writer = new AssemblyWriter();
                var modifiedPath = writer.SaveAssembly(runCtx, asmCtx);
                _logger.Info($"Writed: [{modifiedPath}]");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Processing of file failed: {runCtx.ProcessingFile}");
                if (opts.Debug?.IgnoreErrors != true)
                    throw;
                return false;
            }

            return true;
        }

        internal void DeployInjectedTree(InjectedSolution tree)
        {
            tree.RemoveEmpties();
            tree.FinishTime = DateTime.Now;
            var deployer = new TreeDeployer(_rep);
            deployer.Deploy(tree); //copying tree data to the target root's directories
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

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
            https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C# (buggy and with restrictions!)
                on Github - https://github.com/adrianoc/cecilifier
            https://www.codeproject.com/Articles/671259/Reweaving-IL-code-with-Mono-Cecil
            https://blog.elishalom.com/2012/02/04/monitoring-execution-using-mono-cecil/
            https://stackoverflow.com/questions/48090703/run-mono-cecil-in-net-core

            Mono.Cecil maintainer: jbevain@gmail.com
        */

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
        }

        /*****************************************************************/

        /// <summary>
        /// Inject the target accordingly by the current config from repository
        /// </summary>
        /// <returns>Tree of metadata for injected entities (processed directories, assemblies, 
        /// classes, methods, cross-points, etc)</returns>
        public Task<InjectedSolution> Process()
        {
            return Process(_rep.Options);
        }

        /// <summary>
        ///  Inject the target accordingly by the config form parameters 
        /// </summary>
        /// <param name="opts">Config for target's injection</param>
        /// <returns>Tree data of injection (processed directories, assemblies, classes, methods, cross-points, and their meta-data)</returns>
        public async Task<InjectedSolution> Process(InjectorOptions opts)
        {
            _logger.Debug("Process is starting...");
            InjectorOptionsHelper.ValidateOptions(opts);

            var sourceDir = opts.Source.Directory;
            var destDir = opts.Destination.Directory;

            //copying of all needed data in needed targets
            var monikers = opts.Versions?.Targets;
            _logger.Debug("The source is copying...");
            _rep.CopySource(sourceDir, destDir, monikers); //TODO: copy dirs only according to the filter
            _logger.Info("The source is copied");

            //tree
            var tree = new InjectedSolution(opts.Target?.Name, sourceDir)
            {
                StartTime = DateTime.Now,
                DestinationPath = destDir,
                Description = opts.Description,
            };

            //ctx of this Run
            using var runCtx = new RunContext(_rep, tree);

            //folders: possible targets from cfg
            var dirs = Directory.GetDirectories(sourceDir, "*");
            foreach (var dir in dirs)
            {
                //filter by cfg
                if (!opts.Source.Filter.IsDirectoryNeed(dir))
                    continue;

                //filter by target moniker (typed version)
                var need = monikers == null || monikers.Count == 0 || monikers.Any(a =>
                {
                    var x = Path.Combine(sourceDir, a.Value.BaseFolder);
                    if (x.EndsWith("\\"))
                        x = x.Substring(0, x.Length - 1);
                    if (x.Equals(dir, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                    var z = Path.Combine(dir, a.Key);
                    return x.Equals(z, StringComparison.InvariantCultureIgnoreCase);
                });

                if (need)
                {
                    runCtx.SourceDirectory = dir;
                    await ProcessDirectory(runCtx);
                }
            }

            if (!runCtx.Tree.GetAllAssemblies().Any())
            {
                //files in root
                runCtx.SourceDirectory = runCtx.RootDirectory;
                await ProcessDirectory(runCtx);
            }

            //the tree's deploying
            tree.RemoveEmpties();
            tree.FinishTime = DateTime.Now;
            var deployer = new TreeDeployer(runCtx.Repository);
            deployer.Deploy(tree); //copying tree data to target root directories

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
        /// Process directory from current Engine's context
        /// </summary>
        /// <param name="runCtx">Context of Engine's Run</param>
        /// <returns>Is the directory processed?</returns>
        internal async Task<bool> ProcessDirectory(RunContext runCtx)
        {
            var opts = runCtx.Options;
            var directory = runCtx.SourceDirectory;
            if (!opts.Source.Filter.IsDirectoryNeed(directory))
                return false;
            var folder = new DirectoryInfo(directory).Name;
            var isRoot = runCtx.SourceDirectory == runCtx.RootDirectory;
            if (!isRoot && !opts.Source.Filter.IsFolderNeed(folder))
                return false;
            _logger.Info($"Processing dir [{directory}]");

            //files
            var files = _rep.GetAssemblies(directory);
            foreach (var file in files)
            {
                runCtx.SourceFile = file;
                await ProcessFile(runCtx).ConfigureAwait(false);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*");
            foreach (var dir in dirs)
            {
                runCtx.SourceDirectory = dir;
                await ProcessDirectory(runCtx);
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
            var filePath = runCtx.SourceFile;

            //filter
            if (!opts.Source.Filter.IsFileNeedByPath(Path.GetFileName(filePath)))
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
                await runCtx.Inject(asmCtx);

                _logger.Debug($"Injected: [{runCtx.SourceFile}]");

                //writing modified assembly and symbols to new file
                var writer = new AssemblyWriter();
                var modifiedPath = writer.SaveAssembly(runCtx, asmCtx);
                _logger.Info($"Writed: [{modifiedPath}]");
            }
            catch (Exception ex)
            {
                _logger.Error("Error: {Ex}", ex);
                if (opts.Debug?.IgnoreErrors != true)
                    throw ex;
                return false;
            }

            return true;
        }
    }
}
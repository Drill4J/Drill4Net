﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Injection;
using Drill4Net.Repository;
using Drill4Net.Configuration;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Strategies.Blocks;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Injector Engine's repository (provides injection strategy, target assemblies, 
    /// injector for them, the reading/writing of resulting tree data, etc)
    /// </summary>
    public class InjectorRepository : TreeRepository<InjectionOptions, InjectionOptionsHelper>, IInjectorRepository
    {
        /// <summary>
        /// The options for Injector app itself.
        /// </summary>
        public InjectorAppOptions AppOptions { get; }

        private Logger _logger;
        private const string _subsystem = CoreConstants.SUBSYSTEM_INJECTOR;

        /*****************************************************************************************/

        /// <summary>
        /// Create Injector Engine's repository (provides injection strategy, target assemblies, 
        /// injector for them, the reading/writing of resulting tree data, etc)
        /// </summary>
        /// <param name="cfgPath">Path to the Injector config</param>
        /// <param name="cliDescriptor">Input arguments from console, including path to config, etc</param>
        public InjectorRepository(string cfgPath, CliDescriptor cliDescriptor = null): base(_subsystem, cfgPath, cliDescriptor)
        {
            CreateLogger();
            _optHelper.Clarify(cliDescriptor, Options);
            AppOptions = GetInjectorAppOptions();
        }

        /*****************************************************************************************/

        /// <summary>
        /// Get the concrete assembly injector
        /// </summary>
        /// <returns></returns>
        public IAssemblyInjector GetInjector()
        {
            return new AssemblyInjector(Options, GetStrategy());
        }

        /// <summary>
        /// Get the strategy of Target injections
        /// </summary>
        /// <returns></returns>
        public CodeHandlerStrategy GetStrategy()
        {
            return new BlockStrategy(Options);
        }

        /// <summary>
        /// Get the Injector app config
        /// </summary>
        /// <returns></returns>
        public static InjectorAppOptions GetInjectorAppOptions(string cfgPath = null)
        {
            var optHelper = new BaseOptionsHelper<InjectorAppOptions>();
            var opts = optHelper.ReadOptions(cfgPath ?? GetInjectorAppOptionsPath());
            if (string.IsNullOrWhiteSpace(opts.PluginDir))
                opts.PluginDir = Path.Combine(FileUtils.EntryDir, "plugins");
            opts.PluginDir = FileUtils.GetFullPath(opts.PluginDir);
            //if (!Directory.Exists(opts.PluginDir))
            //    throw new DirectoryNotFoundException("Injector plugin directory is not found");
            return opts;
        }

        /// <summary>
        /// Get Injector App Options path
        /// </summary>
        /// <returns></returns>
        public static string GetInjectorAppOptionsPath()
        {
            return Path.Combine(FileUtils.EntryDir, CoreConstants.CONFIG_NAME_APP);
        }

        private void CreateLogger()
        {
            _logger = new TypedLogger<InjectorRepository>(_subsystem);
        }

        /// <summary>
        /// Get the IL code generator of the injecting Proxy Type
        /// </summary>
        /// <returns></returns>
        public IProfilerProxyInjector GetProxyGenerator()
        {
            var profilerOpts = Options.Profiler;
            var profDir = profilerOpts.Directory;
            return new ProfilerProxyInjector(Options.Proxy.Class, Options.Proxy.Method, //proxy to profiler
                                             profDir, profilerOpts.AssemblyName, //real profiler
                                             profilerOpts.Namespace, profilerOpts.Class, profilerOpts.Method);
        }

        /// <summary>
        /// Preliminary coping the source directories and files from source to destination 
        /// according to the needed monikers (framework versions)
        /// </summary>
        /// <param name="sourcePath">Source directory</param>
        /// <param name="destPath">destintation directory</param>
        /// <param name="monikers">Dictionary of framework versions monikers from VersionOptions.Targets/>.
        /// Key is moniker (for example, net6.0)</param>
        public async virtual Task CopySource(string sourcePath, string destPath, Dictionary<string, MonikerData> monikers)
        {
            if (Directory.Exists(destPath))
            {
                //sometimes deleting directory can blocked itself
                var di = new DirectoryInfo(destPath);
                if (di.GetDirectories().Length > 0 || di.GetFiles().Length > 0)
                {
                    Directory.Delete(destPath, true);
                    Directory.CreateDirectory(destPath);
                }
            }

            if (monikers == null)
            {
                await FileUtils.DirectoryCopy(sourcePath, destPath)
                    .ConfigureAwait(false);
            }
            else
            {
                foreach (var moniker in monikers.Keys)
                {
                    var data = monikers[moniker];
                    var sourcePath2 = Path.Combine(sourcePath, data.BaseFolder);
                    var destPath2 = Path.Combine(destPath, data.BaseFolder);
                    await FileUtils.DirectoryCopy(sourcePath2, destPath2)
                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Validation for options from loaded config
        /// </summary>
        public virtual void ValidateOptions()
        {
            var cfgHelper = new InjectionOptionsHelper();
            cfgHelper.ValidateOptions(Options);
        }

        #region Assembly
        /// <summary>
        /// Get assemblies (.dll and .exe) from the specified directory
        /// </summary>
        /// <param name="directory">Directory with Target assemblies</param>
        /// <returns></returns>
        public virtual IEnumerable<string> GetAssemblies(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(a => a.EndsWith(".exe") || a.EndsWith(".dll"))
                .OrderByDescending(a => a); //better EXE first
        }

        /// <summary>
        /// Try to get framework version of assembly. In some cases,
        /// the version cannot be obtained.
        /// </summary>
        /// <param name="filePath">Path to the file of assembly</param>
        /// <returns>Version of assembly including other metadata</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual AssemblyVersioning TryGetAssemblyVersion(string filePath)
        {
            try
            {
                var asmName = AssemblyName.GetAssemblyName(filePath);
                if (asmName.ProcessorArchitecture == ProcessorArchitecture.None)
                    return new AssemblyVersioning() { FrameworkType = AssemblyVersionType.NotIL };

                //TODO: we don't work with strong names yet (it's possible)
                if (!asmName.FullName.EndsWith("PublicKeyToken=null"))
                {
                    _logger.Warning($"Assembly [{filePath}] having the strong name");
                    return new AssemblyVersioning() { IsStrongName = true };
                }

                return CommonUtils.GetAssemblyVersion(filePath);
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2146234344) //The module was expected to contain an assembly manifest
                    _logger.Warning($"Getting assembly version for [{filePath}]: {ex.Message}");
                return null;
            }
        }
        #endregion
        #region Injected Tree
        /// <summary>
        /// Read the metadata of injected entities from file
        /// </summary>
        /// <returns>Tree data of injected entities (directories, assemblies, types, methods, etc)</returns>
        public virtual InjectedSolution ReadInjectedTree()
        {
            string treePath = null;
            if (Options?.Destination != null)
            {
                var targetDir = Options.Destination.Directory;
                treePath = _helper.GetTreeFilePathByDir(targetDir);
            }
            return ReadInjectedTree(treePath);
        }

        /// <summary>
        /// Write metadata of injected entities (directories, assemblies, types, methods, etc) to the file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tree"></param>
        public virtual void WriteInjectedTree(string path, InjectedSolution tree)
        {
            var data = Serializer.ToArray<InjectedSolution>(tree);
            File.WriteAllBytes(path, data);
        }
        #endregion
    }
}

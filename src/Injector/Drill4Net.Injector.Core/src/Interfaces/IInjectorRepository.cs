using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Injector Engine's repository (provides injection strategy, target assemblies, 
    /// injector for them, the reading/writing of resulting tree data, etc)
    /// </summary>
    public interface IInjectorRepository
    {
        /// <summary>
        /// Options for the injection
        /// </summary>
        InjectorOptions Options { get; set; }

        string Subsystem { get; }

        /// <summary>
        /// Get the concrete assembly injector
        /// </summary>
        /// <returns></returns>
        IAssemblyInjector GetInjector();

        /// <summary>
        /// Get the strategy of Target injections
        /// </summary>
        /// <returns></returns>
        CodeHandlerStrategy GetStrategy();

        /// <summary>
        /// Get the IL code generator of the injecting Proxy Type
        /// </summary>
        /// <returns></returns>
        IProfilerProxyGenerator GetProxyGenerator();

        /// <summary>
        /// Validation for options from loaded config
        /// </summary>
        void ValidateOptions();

        /// <summary>
        /// Preliminary coping the source directories and files from source to destination 
        /// according to the needed monikers (framework versions)
        /// </summary>
        /// <param name="sourcePath">Source directory</param>
        /// <param name="destPath">destintation directory</param>
        /// <param name="monikers">Dictionary of framework versions monikers from <see cref="VersionData.Targets"/>.
        /// Key is moniker (for example, net5.0)</param>
        void CopySource(string sourcePath, string destPath, Dictionary<string, MonikerData> monikers);

        /// <summary>
        /// Get assemblies (.dll and .exe) from the specified directory
        /// </summary>
        /// <param name="directory">Directory with Target assemblies</param>
        /// <returns></returns>
        IEnumerable<string> GetAssemblies(string directory);

        /// <summary>
        /// Try to get framework version of assembly. In some cases,
        /// the version cannot be obtained.
        /// </summary>
        /// <param name="filePath">Path to the file of assembly</param>
        /// <returns>Version of assembly including other metadata</returns>
        AssemblyVersioning TryGetAssemblyVersion(string filePath);

        /// <summary>
        /// Read the metadata of injected entities from file
        /// </summary>
        /// <returns>Tree data of injected entities (directories, assemblies, types, methods, etc)</returns>
        InjectedSolution ReadInjectedTree(string path);

        /// <summary>
        /// Write metadata of injected entities (directories, assemblies, types, methods, etc) to the file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tree"></param>
        void WriteInjectedTree(string path, InjectedSolution tree);

        string GetTreeFilePath(InjectedSolution tree);
        string GetTreeFileHintPath(string path);
        string GetTreeFilePath(string targetDir);
    }
}
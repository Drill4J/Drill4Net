using System.Collections.Generic;
using System.Threading.Tasks;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// High-level Injector Engine working with target directories and files. 
    /// It injects the Agent's proxy code to the needed methods and produces 
    /// the Tree metadata of processed entities (directories, assemblies, 
    /// classes, methods, cross-points, etc)
    /// </summary>
    public interface IInjectorEngine
    {
        List<IInjectorPlugin> Plugins { get; }

        /// <summary>
        /// Inject the target accordingly by the current config from repository
        /// </summary>
        /// <returns>Tree of metadata for the injected entities (processed directories, assemblies,
        /// classes, methods, cross-points, etc)</returns>
        Task<InjectedSolution> Process();

        /// <summary>
        ///  Inject the target accordingly by the config form parameters 
        /// </summary>
        /// <param name="opts">Config for target's injection</param>
        /// <returns>Tree data of the injection (processed directories, assemblies, classes, methods, cross-points, and their meta-data)</returns>
        Task<InjectedSolution> Process(InjectorOptions opts);
    }
}
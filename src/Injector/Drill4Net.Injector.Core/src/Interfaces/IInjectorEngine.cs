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
        Task<InjectedSolution> Process();
        Task<InjectedSolution> Process(InjectorOptions opts);
    }
}
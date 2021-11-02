using System.Threading.Tasks;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorPlugin
    {
        /// <summary>
        /// Unique name of the plugin
        /// </summary>
        string Name { get; }
        Task Process(RunContext runCtx);
    }
}

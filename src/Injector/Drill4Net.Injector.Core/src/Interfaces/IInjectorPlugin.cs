using Mono.Cecil;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorPlugin
    {
        /// <summary>
        /// Unique name of the plugin
        /// </summary>
        string Name { get; }

        void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false);
    }
}

using System;
using Mono.Cecil;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// IL code generator of the injected Profiler's type
    /// </summary>
    public interface IProfilerProxyGenerator: IDisposable
    {
        string ProfilerAsmName { get; }
        string ProfilerClass { get; }
        string ProfilerFunc { get; }
        string ProfilerNs { get; }
        string ProfilerReadDir { get; }
        string ProxyClass { get; }
        string ProxyFunc { get; }

        /// <summary>
        /// Generating IL instructions for a class ProfilerProxy by Mono.Cecil
        /// </summary>
        /// <param name="assembly">The injected assembly</param>
        /// <param name="proxyNs">The Proxy's namespace for current assembly</param>
        /// <param name="isNetFX">Is NetFramework or Net Core?</param>
        void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false);
    }
}
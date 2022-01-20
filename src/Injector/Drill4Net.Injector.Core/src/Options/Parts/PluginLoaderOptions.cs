using System;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for the injecting info about specific runtime plugins
    /// </summary>
    [Serializable]
    public class PluginLoaderOptions
    {
        /// <summary>
        /// Common (shared) plugin assemblies' directory
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Target and plugin specific inner config with running test assembly, etc. It can be
        /// just name for Injector root directory or path (relative or absolute)
        /// </summary>
        public string Config { get; set; }
    }
}

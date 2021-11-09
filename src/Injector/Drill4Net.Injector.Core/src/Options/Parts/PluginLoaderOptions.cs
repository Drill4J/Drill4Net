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
        /// Path to the plugin assemblies
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Inner plugin specific config: name (for Injector's root directory)
        /// or path (relative or absolute)
        /// </summary>
        public string Config { get; set; }
    }
}

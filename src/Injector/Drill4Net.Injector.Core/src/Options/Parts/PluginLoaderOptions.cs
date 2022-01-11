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
        /// Plugin assemblies' directory
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Inner plugin specific config: name (for Injector's root directory)
        /// or path (relative or absolute)
        /// </summary>
        public string Config { get; set; }
    }
}

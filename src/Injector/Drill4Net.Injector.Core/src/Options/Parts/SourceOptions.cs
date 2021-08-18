using System;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for the Source of the Target (instrumenting App) - what and how processing
    /// </summary>
    [Serializable]
    public class SourceOptions
    {
        /// <summary>
        /// Root directory of the injecting Source
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Options for the Filter of directories, folders, namespaces, type names, etc
        /// </summary>
        public SourceFilterOptions Filter { get; set; }
    }
}

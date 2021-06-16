using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Base options for the Drill4Net system generally
    /// </summary>
    public abstract class BaseOptions
    {
        /// <summary>
        /// Descriptive type of config (for Injector, Test Engine, Agent, etc)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Options for the injected target
        /// </summary>
        public TargetOptions Target { get; set; }

        /// <summary>
        /// Path to the Tree data file, if empty, system will try find it 
        /// by another ways using "redirect cfg", current dir, etc
        /// </summary>
        public string TreePath { get; set; }

        /// <summary>
        /// Gets or sets the options for logs.
        /// </summary>
        /// <value>
        /// The logs' options.
        /// </value>
        public List<LogOptions> Logs { get; set; }

        /// <summary>
        /// Description for injection process
        /// </summary>
        public string Description { get; set; }
    }
}

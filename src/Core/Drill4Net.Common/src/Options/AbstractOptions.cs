using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Base abstract options
    /// </summary>
    public abstract class AbstractOptions
    {
        /// <summary>
        /// Descriptive type of the config (for Injector, Test Engine, Agent, etc)
        /// </summary>
        public string Type { get; set; }

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

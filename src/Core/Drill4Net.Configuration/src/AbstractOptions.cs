using System;
using System.Collections.Generic;

namespace Drill4Net.Configuration
{
    /// <summary>
    /// Base abstract options
    /// </summary>
    [Serializable]
    public abstract class AbstractOptions
    {
        /// <summary>
        /// Descriptive type of the config (for Injector, Test Engine, Agent, Server/Worker, etc)
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
        /// Description for the entity config/process/app
        /// </summary>
        public string Description { get; set; }
    }
}

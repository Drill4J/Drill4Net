using Drill4Net.Agent.Messaging;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Options for the Configurator
    /// </summary>
    public class ConfiguratorOptions : MessagerOptions
    {
        public string? AdminHost { get; set; }
        public int AdminPort { get; set; }

        /// <summary>
        /// Default directory for Agent's plugins
        /// </summary>
        public string? PluginDirectory { get; set; }
    }
}

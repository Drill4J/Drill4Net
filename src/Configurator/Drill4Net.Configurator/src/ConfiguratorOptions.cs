using Drill4Net.Agent.Messaging;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Options for the Configurator
    /// </summary>
    public class ConfiguratorOptions : MessagerOptions
    {
        /// <summary>
        /// Default directory for Agent's plugins
        /// </summary>
        public string? PluginDirectory { get; set; }
    }
}

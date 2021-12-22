using Drill4Net.Configuration;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Default options for the Configurator
    /// </summary>
    public class ConfiguratorOptions : AbstractOptions
    {
        /// <summary>
        /// Default host address for the Drill Admin service
        /// </summary>
        public string? AdminHost { get; set; }

        /// <summary>
        /// Default port for the Drill Admin service
        /// </summary>
        public int AdminPort { get; set; }

        /// <summary>
        /// Default host address for the middleware service (Kafka)
        /// </summary>
        public string? MiddlewareHost { get; set; }

        /// <summary>
        /// Default port for the middleware service (Kafka)
        /// </summary>
        public int MiddlewarePort { get; set; }

        /// <summary>
        /// Install directory of Drill4Net system
        /// </summary>
        public string? InstallDirectory { get; set; }

        /// <summary>
        /// Default directory for the Transmitter module
        /// </summary>
        public string? TransmitterDirectory { get; set; }

        /// <summary>
        /// Default directory for the Agent's plugins (IEngineContexters)
        /// </summary>
        public string? PluginDirectory { get; set; }
    }
}

using Drill4Net.Configuration;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Default options for the Configurator
    /// </summary>
    public class ConfiguratorOptions : AbstractOptions
    {
        /// <summary>
        /// Install directory of Drill4Net system
        /// </summary>
        public string? InstallDirectory { get; set; }

        /// <summary>
        /// Default directory for the Injector module
        /// </summary>
        public string? InjectorDirectory { get; set; }

        /// <summary>
        /// Default directory for the Test Runner module
        /// </summary>
        public string? TestRunnerDirectory { get; set; }

        /// <summary>
        /// Default directory for the Transmitter module
        /// </summary>
        public string? TransmitterDirectory { get; set; }

        /// <summary>
        /// Default directory for the Transmitter (agent) plugins
        /// </summary>
        public string? AgentPluginDirectory { get; set; }

        //Hint: on other hand, the Injector plugin dir (IInjectorPlugin) is placed in app.yml for the Injector (in its root)

        /// <summary>
        /// Default directory for the CI runs
        /// </summary>
        public string? CiDirectory { get; set; }

        /// <summary>
        /// Default project sources' directory for CI integrations
        /// </summary>
        public string? ProjectsDirectory { get; set; }

        /// <summary>
        /// External editor to edit the configs
        /// </summary>
        public string? ExternalEditor { get; set; }
    }
}

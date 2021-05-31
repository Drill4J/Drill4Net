namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the Injector App
    /// </summary>
    public class InjectorOptions : BaseOptions
    {
        /// <summary>
        /// Options for the Source of the Target (instrumenting App) - what and how processing
        /// </summary>
        public SourceOptions Source { get; set; }

        /// <summary>
        /// Options for the Destination: path, naming, etc
        /// </summary>
        public DestinationOptions Destination { get; set; }

        public ProfilerOptions Profiler { get; set; }

        public CallerOptions Proxy { get; set; }

        public ProbesOptions Probes { get; set; }

        public VersionOptions Versions { get; set; }

        public bool Silent { get; set; }
    }
}

using System;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for the Injector App
    /// </summary>
    [Serializable]
    public class InjectorOptions : BaseTargetOptions
    {
        /// <summary>
        /// Options for the Source of the Target (instrumenting App) - what and how processing
        /// </summary>
        public SourceOptions Source { get; set; }

        /// <summary>
        /// Options for the Destination: path, naming, etc
        /// </summary>
        public DestinationOptions Destination { get; set; }

        /// <summary>
        /// Options for the Profiler (Agent)
        /// </summary>
        public ProfilerOptions Profiler { get; set; }

        /// <summary>
        /// Options for the Proxy class injected to the Target
        /// </summary>
        public CallerOptions Proxy { get; set; }

        /// <summary>
        /// Options for the Debug mode
        /// </summary>
        public InjectorDebugOptions Debug { get; set; }

        /// <summary>
        /// Options for the injecting process (types of methods, cross-points, etc)
        /// </summary>
        public ProbesOptions Probes { get; set; }

        /// <summary>
        /// Parameters for the target object versions to be processed
        /// </summary>
        public VersionOptions Versions { get; set; }

        /// <summary>
        /// Is the silent mode set (no interactive with defaults for the Injector)?
        /// </summary>
        public bool Silent { get; set; }
    }
}

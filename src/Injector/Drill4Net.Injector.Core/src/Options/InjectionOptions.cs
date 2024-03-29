﻿using System;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for the Injector App
    /// </summary>
    [Serializable]
    public class InjectionOptions : TargetOptions
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
        public InjectionDebugOptions Debug { get; set; }

        /// <summary>
        /// Options for the injecting process (types of methods, cross-points, etc)
        /// </summary>
        public ProbeData Probes { get; set; }

        public Dictionary<string, PluginLoaderOptions> Plugins { get; set; }

        /// <summary>
        /// Parameters for the target object versions to be processed
        /// </summary>
        public VersionData Versions { get; set; }

        /// <summary>
        /// Is the silent mode set (no interactive with defaults for the Injector)?
        /// </summary>
        public bool Silent { get; set; }

        /**********************************************************************************/

        public InjectionOptions()
        {
            Type = CoreConstants.SUBSYSTEM_INJECTOR;
        }
    }
}

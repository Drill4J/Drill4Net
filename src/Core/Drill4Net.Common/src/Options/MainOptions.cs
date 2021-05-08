﻿namespace Drill4Net.Common
{
    public class MainOptions
    {
        public TargetOptions Target { get; set; }
        public SourceOptions Source { get; set; }

        public DestinationOptions Destination { get; set; }

        public ProfilerOptions Profiler { get; set; }

        public AdminOptions Admin { get; set; }

        public CallerOptions Proxy { get; set; }

        public ProbesOptions Probes { get; set; }

        public TestsOptions Tests { get; set; }

        public bool Silent { get; set; }

        public string Description { get; set; }
    }
}

namespace Drill4Net.Common
{
    public class InjectorOptions : BaseOptions
    {
        public SourceOptions Source { get; set; }

        public DestinationOptions Destination { get; set; }

        public ProfilerOptions Profiler { get; set; }

        public CallerOptions Proxy { get; set; }

        public ProbesOptions Probes { get; set; }

        public VersionOptions Versions { get; set; }

        public bool Silent { get; set; }
    }
}

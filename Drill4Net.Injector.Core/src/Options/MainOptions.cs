namespace Drill4Net.Injector.Core
{
    public class MainOptions
    {
        public SourceOptions Source { get; set; }

        public DestinationOptions Destination { get; set; }

        public ProfilerOptions Profiler { get; set; }

        public CallerOptions Proxy { get; set; }

        public ProbesOptions Probes { get; set; }

        public TestsOptions Tests { get; set; }
    }
}

namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the Profiler (Agent)
    /// </summary>
    public class ProfilerOptions : CallerOptions
    {
        /// <summary>
        /// Descriptive name for the Agent (optional)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full path to the folder where Profier is located
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Short file name of the Profiler with extension
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Namespace of the Profiler's class
        /// </summary>
        public string Namespace { get; set; }
    }
}

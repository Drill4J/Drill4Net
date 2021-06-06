namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the Tester Subsystem for the Project Solution
    /// </summary>
    public class TesterOptions : BaseOptions
    {
        /// <summary>
        /// Versions of Terget for teh testing
        /// </summary>
        public VersionOptions Versions { get; set; }

        /// <summary>
        /// Options for the injecting process (types of methods, cross-point, etc)
        /// </summary>
        public ProbesOptions Probes { get; set; }
    }
}

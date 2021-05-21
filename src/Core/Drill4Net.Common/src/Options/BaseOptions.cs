namespace Drill4Net.Common
{
    /// <summary>
    /// Base options for the Drill4Net system generally
    /// </summary>
    public class BaseOptions
    {
        /// <summary>
        /// Descriptive type of config (for Injector, Test Engine, Agent, etc)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Options for the injected target
        /// </summary>
        public TargetOptions Target { get; set; }

        /// <summary>
        /// Path to the Tree data file, if empty, system will try find it 
        /// by another ways using "redirect cfg", current dir, etc
        /// </summary>
        public string TreePath { get; set; }

        /// <summary>
        /// Description for injection process
        /// </summary>
        public string Description { get; set; }
    }
}

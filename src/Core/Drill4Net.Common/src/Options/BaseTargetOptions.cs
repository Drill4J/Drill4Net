namespace Drill4Net.Common
{
    /// <summary>
    /// Base options for the Drill4Net system generally
    /// </summary>
    public abstract class BaseTargetOptions : AbstractOptions
    {
        /// <summary>
        /// Options for the injected target
        /// </summary>
        public TargetOptions Target { get; set; }

        /// <summary>
        /// Path to the Tree data file, if empty, system will try find it 
        /// by another ways using "redirect cfg", current dir, etc
        /// </summary>
        public string TreePath { get; set; }
    }
}

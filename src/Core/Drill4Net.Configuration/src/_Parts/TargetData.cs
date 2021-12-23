using System;

namespace Drill4Net.Configuration
{
    /// <summary>
    /// Metadata about Target
    /// </summary>
    [Serializable]
    public class TargetData
    {
        public string Name { get; set; }
        public string Version { get; set; }

        /// <summary>
        ///  Name of main Product assembly - if the <see cref="Version"/> is empty, 
        ///  you can specify with the automatic changing version
        /// </summary>
        public string VersionAssemblyName { get; set; }
    }
}
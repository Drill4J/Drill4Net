using System;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Options for the Tester app
    /// </summary>
    [Serializable]
    public class TesterOptions
    {
        /// <summary>
        /// Current directory where Target's assemblies are located, mandatory field.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Inner folder in tree data, if tree contains data for several framework's versions.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public string Moniker { get; set; }

        /// <summary>
        /// Directory where the tree data must be saved. The empty field means local directory
        /// </summary>
        public string CSV { get; set; }
    }
}

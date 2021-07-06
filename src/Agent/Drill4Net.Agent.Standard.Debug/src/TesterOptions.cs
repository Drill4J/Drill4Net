using System;

namespace Drill4Net.Agent.Standard.Debug
{
    [Serializable]
    public class TesterOptions
    {
        /// <summary>
        /// CUrrent directory where Target's assemblies is located, mandatory filed.
        /// </summary>
        public string CurrentDirectory { get; set; }

        /// <summary>
        /// Inner folder in tree data, if tree contains data for several framework's versions.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public string TreeFolder { get; set; }

        /// <summary>
        /// Directory where the tree data must be saved. The empty field means local directory
        /// </summary>
        public string CSV { get; set; }
    }
}

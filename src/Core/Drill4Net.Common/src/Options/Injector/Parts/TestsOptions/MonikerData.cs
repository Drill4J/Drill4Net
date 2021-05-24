using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Data for moniker of framework version (netcoreapp3.1, net5.0, etc)
    /// </summary>
    public class MonikerData
    {
        /// <summary>
        /// Base folder(s) of the testing target
        /// </summary>
        public string BaseFolder { get; set; }

        /// <summary>
        /// Testing folders in the BaseFolder
        /// </summary>
        public List<FolderData> Folders { get; set; }
    }
}

using System.Collections.Generic;

namespace Drill4Net.Common
{
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

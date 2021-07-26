using System;
using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Data for assemblies and it's classes in folder for testing 
    /// </summary>
    [Serializable]
    public class FolderData
    {
        /// <summary>
        /// Current folder(s) of the testing target
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Testing assemblies in the Folder: key is name of assembly's file, 
        /// for example, "Drill4Net.Target.Common.dll", value is list 
        /// of full class names (Namespace.Name)
        /// </summary>
        public Dictionary<string, List<string>> Assemblies { get; set; }

        /************************************************************************/

        public override string ToString()
        {
            return Folder;
        }
    }
}

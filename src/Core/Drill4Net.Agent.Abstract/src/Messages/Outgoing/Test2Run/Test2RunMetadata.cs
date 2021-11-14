using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Metadata about test case
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public class Test2RunMetadata
    {
        /// <summary>
        /// Don't used yet. In theory, this should be a hash of the test name.
        /// </summary>
        public string hash { get; set; }

        /// <summary>
        /// Some real metadata
        /// </summary>

        public Dictionary<string, string> data { get; set; }

        /***************************************************************************************/

        public override string ToString()
        {
            if (data == null)
                return null;
            return JsonConvert.SerializeObject(data);
        }
    }
}

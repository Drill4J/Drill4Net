using System;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public class ProfilingData
    {
        /// <summary>
        /// Get the method full name (by Injector standard) against Guid of cross-point (probe).
        /// </summary>
        public Dictionary<Guid, string> MethodByProbUid { get; set; }

        /// <summary>
        /// Get the AstMethod for transfer to the API by full name (by Injector standard).
        /// </summary>
        public Dictionary<string, AstMethod> AstMethodByName { get; set; }

        /*****************************************************************************/

        public ProfilingData()
        {
            MethodByProbUid = new Dictionary<Guid, string>();
            AstMethodByName = new Dictionary<string, AstMethod>();
        }
    }
}

using System;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// The profiling data: maps for methods including its DTO presentation (<see cref="AstMethod"/>)
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilingData"/> class.
        /// </summary>
        public ProfilingData()
        {
            MethodByProbUid = new Dictionary<Guid, string>();
            AstMethodByName = new Dictionary<string, AstMethod>();
        }
    }
}

﻿using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Data repository for the profiling Agent
    /// </summary>
    public class AgentRepository : TreeRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>
    {
        /// <summary>
        /// Name of Target app
        /// </summary>
        public string TargetName { get; protected set; }

        /// <summary>
        /// Target app's version
        /// </summary>
        public string TargetVersion { get; protected set; }

        /// <summary>
        /// Communicator for transfer probe data to admin side
        /// </summary>
        public AbstractCommunicator Communicator { get; set; }

        /****************************************************************************/

        public AgentRepository(string cfgPath = null) : base(CoreConstants.SUBSYSTEM_AGENT, cfgPath)
        {
        }

        public AgentRepository(AgentOptions opts) : base(CoreConstants.SUBSYSTEM_AGENT, opts)
        {
        }
    }
}
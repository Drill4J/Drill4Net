using System;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Standard
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    public class AgentSender
    {
        private readonly ISender _sender;

        /************************************************************************/

        public AgentSender(ISender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /************************************************************************/

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public void SendInitializedMessage()
        {
            const string data = "Initialized"; //can be any string "Initialized"

        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public void SendClassesDataMessage(IEnumerable<AstEntity> entities)
        {

        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public void SendCoverageData(List<ExecClassData> data)
        {
            
        }

        //???
        public void SendSessionFinishedMessage()
        {
            
        }
    }
}
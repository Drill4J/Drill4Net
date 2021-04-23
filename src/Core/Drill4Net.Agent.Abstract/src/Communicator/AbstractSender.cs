using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    public abstract class AbstractSender : ISender
    {
        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            Send(AgentConstants.MESSAGE_INITIALIZED, "Initialized"); //can be any string
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(IEnumerable<AstEntity> entities)
        {
            Send(AgentConstants.MESSAGE_INIT_DATA_PART, "Initialized");
        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(List<ExecClassData> data)
        {

        }

        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            //"SESSION_FINISHED"
        }

        public abstract void Send(string messageType, string message);
    }
}
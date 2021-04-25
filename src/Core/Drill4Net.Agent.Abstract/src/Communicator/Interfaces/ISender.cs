using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    public interface ISender
    {
        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        void SendInitializedMessage();

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        void SendClassesDataMessage(List<AstEntity> entities);

        void SendSessionStartedMessage(string sessionUid, long ts);
        void SendSessionFinishedMessage(string sessionUid, long ts);
        void SendAllSessionFinishedMessage(List<string> sessionUids, long ts);
        void SendSessionCancelledMessage(string uid, long ts);
        void SendAllSessionCancelledMessage(List<string> uids, long ts);

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        void SendCoverageData(string sessionUid, List<ExecClassData> data);

        void SendSessionChangedMessage(string sessionUid, int probeCount);
        void Send(AbstractOutgoingMessage message);
        void SendTest(IncomingMessage message);
        void SendTest(AbstractOutgoingMessage data);
    }
}
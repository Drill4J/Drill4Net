using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    public interface IAgentCoveragerSender
    {
        void SendScopeInitialized(InitActiveScope scope, long ts);
        
        void SendInitMessage(int classesCount);

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        void SendInitializedMessage();

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        void SendClassesDataMessage(List<AstEntity> entities);

        void SendSessionStartedMessage(string sessionUid, string testType, bool isRealTime, long ts);
        void SendSessionFinishedMessage(string sessionUid, long ts);
        void SendAllSessionFinishedMessage(List<string> sessionUids, long ts);
        void SendSessionCancelledMessage(string uid, long ts);
        void SendAllSessionCancelledMessage(List<string> uids, long ts);

        void SendFinishScopeAction();

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        void SendCoverageData(string sessionUid, List<ExecClassData> data);

        void SendSessionChangedMessage(string sessionUid, int probeCount);
        void Send(string topic, AbstractMessage message);
        
        //for local tests
        void DebugSendOutgoingTest(OutgoingMessage data);
        void DebugSendOutgoingTest(string topic, OutgoingMessage data);
        void DebugSendIncomingTest(string topic, IncomingMessage message);

        void SendStartSessionCommand(string name);
        void SendStopSessionCommand(string name);

        void RegisterTestCaseStart(TestCaseContext testCtx);
        void RegisterTestCaseFinish(TestCaseContext testCtx);
    }
}
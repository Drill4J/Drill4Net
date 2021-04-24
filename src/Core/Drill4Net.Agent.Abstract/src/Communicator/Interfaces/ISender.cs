using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public interface ISender
    {
        void Send(string messageType, object message);
        void SendTest(string messageType, object data);
        
        void SendClassesDataMessage(IEnumerable<AstEntity> entities);
        void SendCoverageData(List<ExecClassData> data);
        void SendInitializedMessage();
        void SendSessionFinishedMessage(string sessionUid, long ts);
        void SendSessionStartedMessage(string uid, long ts);
        void SendCancelAllSessionsMessage(long getCurrentUnixTimeMs);
        void SendSessionCanceledMessage(string uid, long getCurrentUnixTimeMs);
        void SendStopAllSessionsMessage(long getCurrentUnixTimeMs);
    }
}
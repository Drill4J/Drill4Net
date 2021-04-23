using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    public interface ISender
    {
        void Send(string messageType, string message);
        void SendClassesDataMessage(IEnumerable<AstEntity> entities);
        void SendCoverageData(List<ExecClassData> data);
        void SendInitializedMessage();
        void SendSessionFinishedMessage(string sessionUid, long ts);
    }
}
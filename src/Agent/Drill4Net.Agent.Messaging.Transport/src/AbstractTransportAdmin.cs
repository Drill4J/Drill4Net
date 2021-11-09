using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging.Transport
{
    /// <summary>
    /// Base class for message broker administration
    /// </summary>
    public abstract class AbstractTransportAdmin
    {
        public abstract List<string> GetAllTopics(IEnumerable<string> brokerList);
        public abstract void DeleteTopics(IEnumerable<string> servers, IEnumerable<string> topicNameList);
    }
}

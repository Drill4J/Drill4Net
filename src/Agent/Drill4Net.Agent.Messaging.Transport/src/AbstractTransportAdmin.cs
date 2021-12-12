using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging.Transport
{
    /// <summary>
    /// Base class for message broker administration
    /// </summary>
    public abstract class AbstractTransportAdmin
    {
        public abstract List<string> GetAllTopics(IEnumerable<string> brokerList = null);
        public abstract void DeleteTopics(IEnumerable<string> topicNameList, IEnumerable<string> servers = null);
    }
}

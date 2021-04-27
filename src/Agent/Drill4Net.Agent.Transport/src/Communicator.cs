using System;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    public class Communicator : AbstractCommunicator
    {
        public Communicator(string url, AgentPartConfig agentCfg)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            if(agentCfg == null)
                throw new ArgumentNullException(nameof(agentCfg));
            //
            var connector = new Connector(url, agentCfg);
            Receiver = new AgentReceiver(connector);
            Sender = new AgentSender(connector);
        }
    }
}

using System;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    public class Communicator : AbstractCommunicator
    {
        public AgentPartConfig AgentConfig { get; }
        private string Url { get; }

        private readonly Connector _connector;

        /********************************************************************/

        public Communicator(string url, AgentPartConfig agentCfg)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            AgentConfig = agentCfg ?? throw new ArgumentNullException(nameof(agentCfg));

            _connector = new Connector();
            Receiver = new AgentReceiver(_connector);
            Sender = new AgentSender(_connector);
        }

        /********************************************************************/

        public override void Connect()
        {
            _connector.Connect(Url, AgentConfig);
        }
    }
}

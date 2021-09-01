using System;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    /// <summary>
    /// Communicator for interaction with Drill Admin side
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.AbstractCommunicator" />
    public class Communicator : AbstractCommunicator
    {
        /// <summary>
        /// Config for admin side about Agent and instrumented application instances,
        /// applied during connect.
        /// </summary>
        /// <value>
        /// The agent configuration.
        /// </value>
        public AgentPartConfig AgentConfig { get; }

        /// <summary>
        /// Gets the URL of the Drill Admin side.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        private string Url { get; }

        private readonly Connector _connector;
        private readonly Logger _logger;

        /********************************************************************/

        public Communicator(string subsystem, string url, AgentPartConfig agentCfg)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            AgentConfig = agentCfg ?? throw new ArgumentNullException(nameof(agentCfg));
            _logger = new TypedLogger<Communicator>(subsystem);

            _connector = new Connector();
            Receiver = new AgentReceiver(_connector);
            Sender = new AgentSender(_connector);
        }

        /********************************************************************/

        public override void Connect()
        {
            _logger.Info("Connect");
            _connector.Connect(Url, AgentConfig);
        }
    }
}

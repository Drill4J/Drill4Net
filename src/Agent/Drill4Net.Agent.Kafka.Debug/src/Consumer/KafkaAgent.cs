using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class KafkaAgent
    {
        public event ReceivedMessageHandler MessageReceived;
        public event ErrorOccuredHandler ErrorOccured;

        private readonly IProbeConsumer _agent;

        /***************************************************************************/

        public KafkaAgent(IProbeConsumer agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            agent.MessageReceived += Agent_MessageReceived;
            agent.ErrorOccured += Agent_ErrorOccured;
        }

        /***************************************************************************/

        public void Start()
        {
            _agent.Consume();
        }

        private void Agent_MessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }

        private void Agent_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}

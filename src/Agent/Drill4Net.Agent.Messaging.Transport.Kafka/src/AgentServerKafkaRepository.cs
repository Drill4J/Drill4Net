namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class AgentServerKafkaRepository : AbstractAgentServerRepository
    {
        public AgentServerKafkaRepository(string subsystem, string cfgPath = null) : base(subsystem, cfgPath)
        {
        }

        public AgentServerKafkaRepository(string subsystem, AgentServerOptions opts) : base(subsystem, opts)
        {
        }

        public override AbstractTransportAdmin GetTransportAdmin()
        {
            return new TransportKafkaAdmin(Options.Servers);
        }
    }
}

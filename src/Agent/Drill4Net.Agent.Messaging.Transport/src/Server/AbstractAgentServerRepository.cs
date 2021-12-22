namespace Drill4Net.Agent.Messaging.Transport
{
    public abstract class AbstractAgentServerRepository : OptionsRepository<AgentServerOptions>
    {
        protected AbstractAgentServerRepository(string subsystem, string cfgPath = null) : base(subsystem, cfgPath)
        {
        }

        protected AbstractAgentServerRepository(string subsystem, AgentServerOptions opts) : base(subsystem, opts)
        {
        }

        /*****************************************************************************/

        public abstract AbstractTransportAdmin GetTransportAdmin();
    }
}

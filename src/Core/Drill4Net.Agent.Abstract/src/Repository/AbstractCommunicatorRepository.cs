namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractCommunicatorRepository : AgentRepository, IAgentRepository
    {
        /// <summary>
        /// Communicator for transfer probe data to admin side
        /// </summary>
        public AbstractCommunicator Communicator { get; set; }

        public AbstractCommunicatorRepository(string cfgPath = null) : base(cfgPath)
        {
        }
    }
}

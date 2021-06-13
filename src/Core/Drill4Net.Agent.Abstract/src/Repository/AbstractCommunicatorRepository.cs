namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Abstract data repository of the profiling Agent communicating with some Admin side
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.AgentRepository" />
    /// <seealso cref="Drill4Net.Agent.Abstract.IAgentRepository" />
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

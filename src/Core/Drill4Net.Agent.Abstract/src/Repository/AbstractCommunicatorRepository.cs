namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Abstract data repository of the profiling Agent communicating with some Admin side.
    /// The repository of a concrete agent can inherit this class.
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.AgentRepository" />
    /// <seealso cref="Drill4Net.Agent.Abstract.ICommunicatorRepository" />
    public abstract class AbstractCommunicatorRepository : AgentRepository, ICommunicatorRepository
    {
        /// <summary>
        /// Communicator for transfer probe data to admin side
        /// </summary>
        public AbstractCommunicator Communicator { get; set; }

        /**********************************************************************************/

        protected AbstractCommunicatorRepository(string cfgPath = null) : base(cfgPath)
        {
        }

        protected AbstractCommunicatorRepository(AgentOptions opts) : base(opts)
        {
        }
    }
}

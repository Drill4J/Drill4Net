namespace Drill4Net.Agent.Abstract
{
    public interface IAgentRepository
    {
        /// <summary>
        /// Communicator for transfer probe data to admin side
        /// </summary>
        AbstractCommunicator Communicator { get; }
    }
}

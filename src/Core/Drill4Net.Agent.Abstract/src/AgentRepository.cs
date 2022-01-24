using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Data repository for the profiling Agent
    /// </summary>
    public class AgentRepository : TreeRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>
    {
        /// <summary>
        /// Name of Target app
        /// </summary>
        public string TargetName { get; protected set; }

        /// <summary>
        /// Target app's version
        /// </summary>
        public string TargetVersion { get; protected set; }

        /// <summary>
        /// Communicator for transfer probe data to admin side
        /// </summary>
        public AbstractCommunicator Communicator { get; set; }

        protected ContextDispatcher _ctxDisp;

        /****************************************************************************/

        public AgentRepository(string cfgPath = null) : base(CoreConstants.SUBSYSTEM_AGENT, cfgPath)
        {
            Init();
        }

        public AgentRepository(AgentOptions opts) : base(CoreConstants.SUBSYSTEM_AGENT, opts)
        {
            Init();
        }

        /***************************************************************************/

        internal void Init()
        {
            if(AgentInitParameters.LocatedInWorker)
                _ctxDisp = new ContextDispatcher(CoreConstants.SUBSYSTEM_AGENT_WORKER);
            else
                _ctxDisp = new ContextDispatcher(Options.PluginDir, CoreConstants.SUBSYSTEM_AGENT);
        }

        /// <summary>
        /// Get context only for local Agent injected directly in Target's sys process
        /// </summary>
        /// <returns></returns>
        public string GetContextId()
        {
            return _ctxDisp?.GetContextId();
        }

        /// <summary>
        /// Register specified command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public void RegisterCommand(int command, string data)
        {
            _ctxDisp?.RegisterCommand(command, data);
        }

        /// <summary>
        /// Deserialize <see cref="TestCaseContext"/> from JSON string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public TestCaseContext GetTestCaseContext(string str)
        {
            return JsonConvert.DeserializeObject<TestCaseContext>(str);
        }
    }
}

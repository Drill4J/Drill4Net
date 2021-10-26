namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=986565061

    /// <summary>
    /// Constants for Agent subsystem
    /// </summary>
    public static class AgentConstants
    {
        public const string TEST_NAME_DEFAULT = "Default";

        public const string TEST_AUTO = "AUTO";
        public const string TEST_MANUAL = "MANUAL";

        public const string ADMIN_PLUGIN_NAME = "test2code";
        public const string KEY_TESTCASE_CONTEXT = "TestCase_Context";

        public const string CONNECTOR_LOG_FILE_NAME = "connector.log";

        #region Topics
        /// <summary>
        /// The topic is used for agent attaching to the Admin Panel,
        /// agent registration, agent system settings changing.
        /// </summary>
        public const string TOPIC_HEADER_CHANGE = "/agent/change-header-name";

        /// <summary>
        /// The Admin Panel uses the topic for agent registration
        /// with plugins and attaching registered with plugins agent.
        /// </summary>
        public const string TOPIC_AGENT_LOAD = "/agent/load";

        /// <summary>
        /// The topic is used for agent registration with/without a plugin, package settings changing.
        /// </summary>
        public const string TOPIC_CLASSES_LOAD = "/agent/load-classes-data";

        /// <summary>
        /// The topic is used for package settings changing (namespaces),
        /// agent registration with/without plugins.
        /// </summary>
        public const string TOPIC_AGENT_NAMESPACES = "/agent/set-packages-prefixes";

        /// <summary>
        /// The topic is used to send some action to the plugin(s) (e.g. switch active scope,
        /// rename scope, switch scope, delete scope, start a new session, cancel a session, stop a session).
        /// </summary>
        public const string TOPIC_PLUGIN_ACTION = "/plugin/action";

        /// <summary>
        /// The topic is used for plugins disabling, plugins activating (agent registration,
        /// agent attachment, agent toggle, agent system settings changing).
        /// </summary>
        public const string TOPIC_TOGGLE_PLUGIN = "/plugin/togglePlugin";
        #endregion
        #region Message incoming
        public const string MESSAGE_IN_START_SESSION = "START_AGENT_SESSION";
        public const string MESSAGE_IN_INIT_ACTIVE_SCOPE = "INIT_ACTIVE_SCOPE"; 
        public const string MESSAGE_IN_STOP_SESSION = "STOP";
        public const string MESSAGE_IN_STOP_ALL = "STOP_ALL";
        public const string MESSAGE_IN_CANCEL_SESSION = "CANCEL";
        public const string MESSAGE_IN_CANCEL_ALL = "CANCEL_ALL";
        #endregion
        #region Messages outgoing
        public const string MESSAGE_OUT_INIT = "INIT";
        public const string MESSAGE_OUT_INITIALIZED = "INITIALIZED";
        public const string MESSAGE_OUT_INIT_DATA_PART = "INIT_DATA_PART";
        public const string MESSAGE_OUT_SCOPE_INITIALIZED = "SCOPE_INITIALIZED";
        public const string MESSAGE_OUT_SESSION_STARTED = "SESSION_STARTED";
        
        public const string MESSAGE_OUT_COVERAGE_DATA_PART = "COVERAGE_DATA_PART";
        public const string MESSAGE_OUT_SESSION_CHANGED = "SESSION_CHANGED";
        
        public const string MESSAGE_OUT_SESSION_CANCELLED = "SESSION_CANCELLED";
        public const string MESSAGE_OUT_SESSION_ALL_CANCELLED = "SESSIONS_CANCELLED";
        public const string MESSAGE_OUT_SESSION_FINISHED = "SESSION_FINISHED";
        public const string MESSAGE_OUT_SESSION_ALL_FINISHED = "SESSIONS_FINISHED";
        #endregion

    }
}
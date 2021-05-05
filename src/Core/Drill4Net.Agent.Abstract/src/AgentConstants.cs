namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Constants for Agent subsystem
    /// </summary>
    public static class AgentConstants
    {
        public const string TEST_NAME_DEFAULT = "Default";
        public const string ADMIN_PLUGIN_NAME = "test2code";

        #region Topics
        public const string TOPIC_HEADER_CHANGE = "/agent/change-header-name";
        public const string TOPIC_AGENT_LOAD = "/agent/load";
        public const string TOPIC_CLASSES_LOAD = "/agent/load-classes-data";
        public const string TOPIC_AGENT_NAMESPACES = "/agent/set-packages-prefixes";
        public const string TOPIC_PLUGIN_ACTION = "/plugin/action";
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
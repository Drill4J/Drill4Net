namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Constants for Agent subsystem
    /// </summary>
    public static class AgentConstants
    {
        public const string TEST_NAME_DEFAULT = "Default";

        #region Message incoming
        public const string MESSAGE_IN_START_SESSION = "START_AGENT_SESSION";
        public const string MESSAGE_IN_ADD_SESSION_DATA = "ADD_SESSION_DATA"; //???
        public const string MESSAGE_IN_STOP = "STOP";
        public const string MESSAGE_IN_STOP_ALL = "STOP_ALL";
        public const string MESSAGE_IN_CANCEL = "CANCEL";
        public const string MESSAGE_IN_CANCEL_ALL = "CANCEL_ALL";
        #endregion
        #region Messages outgoing
        public const string MESSAGE_OUT_INITIALIZED = "INITIALIZED";
        public const string MESSAGE_OUT_INIT_DATA_PART = "INIT_DATA_PART";
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
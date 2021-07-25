namespace Drill4Net.Common
{
    /// <summary>
    /// Core constants of whole Drill4Net system
    /// </summary>
    public static class CoreConstants
    {
        #region Subsystems
        public const string SUBSYSTEM_INJECTOR = "Injector";
        public const string SUBSYSTEM_TESTER = "Tester";
        public const string SUBSYSTEM_AGENT = "Agent";
        public const string SUBSYSTEM_TRANSMITTER = "ProbeTransmitter";
        //public const string SUBSYSTEM_CONVERTER = "ProbeConverter";
        #endregion
        #region Message headers
        public const string HEADER_MESSAGE_TYPE = "TYPE";
        public const string HEADER_SUBSYSTEM = "SUBSYSTEM";
        public const string HEADER_TARGET = "TARGET";
        #endregion
        #region Message type
        public const string MESSAGE_TYPE_PROBE = "Probe";
        public const string MESSAGE_TYPE_TARGET_INFO = "Target";
        #endregion

        /// <summary>
        /// Name of the file for the redirecting to the certain config
        /// </summary>
        public const string CONFIG_REDIRECT_NAME = "_redirect.yml";

        /// <summary>
        /// Standard file name for the Injector's config (if no <see cref="CONFIG_REDIRECT_NAME"/> and other configs exists)
        /// </summary>
        public const string CONFIG_DEFAULT_NAME = "cfg.yml";

        /// <summary>
        /// Config file name for the Tester subsystem
        /// </summary>
        public const string CONFIG_TESTS_NAME = "tests_cfg.yml";

        public const string CONFIG_SERVICE_NAME = "svc.yml";

        /// <summary>
        /// File name for the injected entities' metadata (files, types, classes, methods, cross-points, etc)
        /// </summary>
        public const string TREE_FILE_NAME = "injected.tree";

        /// <summary>
        /// File name with a path to the <see cref="TREE_FILE_NAME"/> helping to search it
        /// </summary>
        public const string TREE_FILE_HINT_NAME = "injected_hint.tree";

        public const string ARGUMENT_CONFIG_PATH = "cfg_path";
    }
}

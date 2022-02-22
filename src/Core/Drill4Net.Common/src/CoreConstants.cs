namespace Drill4Net.Common
{
    /// <summary>
    /// Core constants of whole Drill4Net system
    /// </summary>
    public static class CoreConstants
    {
        #region Subsystems
        public const string SUBSYSTEM_INJECTOR = "Injector";
        public const string SUBSYSTEM_INJECTOR_APP = "InjectorApp";
        public const string SUBSYSTEM_INJECTOR_PLUGIN = "InjectorPlugin";
        public const string SUBSYSTEM_TESTER = "Tester";
        public const string SUBSYSTEM_TEST_SERVER = "TestServer";
        public const string SUBSYSTEM_TEST_RUNNER = "TestRunner";
        public const string SUBSYSTEM_AGENT = "Agent";
        public const string SUBSYSTEM_TRANSMITTER = "Transmitter";
        public const string SUBSYSTEM_AGENT_SERVER = "AgentServer";
        public const string SUBSYSTEM_AGENT_WORKER = "AgentWorker";
        public const string SUBSYSTEM_CONFIGURATOR = "Configurator";
        public const string SUBSYSTEM_CI = "CI";
        #endregion
        #region Monikers
        public const string MONIKER_NET60 = "net6.0";
        public const string MONIKER_NET50 = "net5.0";

        public const string MONIKER_CORE31 = "netcoreapp3.1";
        public const string MONIKER_CORE22 = "netcoreapp2.2";

        public const string MONIKER_STANDARD20 = "netstandard2.0";
        public const string MONIKER_STANDARD21 = "netstandard2.1";

        public const string MONIKER_NET48 = "net48";
        public const string MONIKER_NET461 = "net461";
        #endregion
        #region CONFIG & TREE
        /// <summary>
        /// Name of the file for the redirecting to the certain config
        /// </summary>
        public const string CONFIG_NAME_REDIRECT = "_redirect.yml";

        /// <summary>
        /// Standard file name for the Injector config (if no <see cref="CONFIG_NAME_REDIRECT"/> and other configs exists)
        /// </summary>
        public const string CONFIG_NAME_DEFAULT = "cfg.yml";

        public const string CONFIG_NAME_APP = "app.yml";

        /// <summary>
        /// Config file name for the Tester subsystem
        /// </summary>
        public const string CONFIG_NAME_TESTS = "tests_cfg.yml";

        public const string CONFIG_NAME_MIDDLEWARE = "svc.yml";

        /// <summary>
        /// File name for the injected entities' metadata (files, types, classes, methods, cross-points, etc)
        /// </summary>
        public const string TREE_FILE_NAME = "injected.tree";

        /// <summary>
        /// File name with a path to the <see cref="TREE_FILE_NAME"/> helping to search it
        /// </summary>
        public const string TREE_FILE_HINT_NAME = "injected_hint.tree";
        #endregion
        #region Environment variables (Docker)
        //Environment variable names are case-sensitive on Linux and macOS but are not case-sensitive on Windows.

        /// <summary>
        /// Environment variables in the Docker for middleware (Kafka)
        /// </summary>
        public const string ENV_MESSAGE_SERVER_ADDRESS = "MESSAGE_SERVER_ADDRESS";

        /// <summary>
        /// Environment variables in the Docker for Drill admin service
        /// </summary>
        public const string ENV_DRILL_ADMIN_ADDRESS = "DRILL_ADMIN_ADDRESS";

        public const string ENV_DEBUG_WRITE_PROBES = "DEBUG_WRITE_PROBES";
        public const string ENV_DEBUG_TOPICS_DONT_DELETE = "DEBUG_TOPICS_DONT_DELETE";
        #endregion
        #region CLI
        public const char SWITCH_FORCE = 'f';

        /// <summary>
        /// If you need to re-run ALL the tests on the same assembly's build
        /// </summary>
        public const char SWITCH_FORCE_RUNNIG_TYPE_ALL = 'A';

        public const string ARGUMENT_SILENT = "silent";
        public const string ARGUMENT_TARGET_NAME = "name";
        public const string ARGUMENT_TARGET_VERSION = "version";
        public const string ARGUMENT_TARGET_VERSIONS = "versions";
        public const string ARGUMENT_CONFIG_PATH = "cfg_path";
        public const string ARGUMENT_CONFIG_DIR = "cfg_dir";
        public const string ARGUMENT_SOURCE_DIR = "source_dir";
        public const string ARGUMENT_SOURCE_PATH = "source_path";
        public const string ARGUMENT_DESTINATION_DIR = "dest_dir";
        public const string ARGUMENT_DESTINATION_PATH = "dest_path";
        public const string ARGUMENT_RUN_CFG = "run_cfg";
        public const string ARGUMENT_POSTFIX = "postfix";
        public const string ARGUMENT_DEGREE_PARALLELISM = "degree_parallelism";
        #endregion

        /// <summary>
        /// Prefix for regex filter in config
        /// </summary>
        public const string REGEX_FILTER_PREFIX = "reg:";
    }
}

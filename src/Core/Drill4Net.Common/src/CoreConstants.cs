namespace Drill4Net.Common
{
    /// <summary>
    /// Core constants of whole Drill4Net system
    /// </summary>
    public static class CoreConstants
    {
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

        /// <summary>
        /// File name for the injected entities' metadata (files, types, classes, methods, cross-points, etc)
        /// </summary>
        public const string TREE_FILE_NAME = "injected.tree";

        /// <summary>
        /// File name with a path to the <see cref="TREE_FILE_NAME"/> helping to search it
        /// </summary>
        public const string TREE_FILE_HINT_NAME = "injected_hint.tree";

        public const string ARGUMENT_CONFIG_PATH = "cfg_path";

        #region Subsystems
        public const string SUBSYSTEM_INJECTOR = "Injector";
        public const string SUBSYSTEM_TESTER = "Tester";
        public const string SUBSYSTEM_AGENT = "Agent";
        #endregion
    }
}

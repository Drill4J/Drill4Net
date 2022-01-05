namespace Drill4Net.Configurator
{
    /// <summary>
    /// Constants for Configurator
    /// </summary>
    public static class ConfiguratorConstants
    {
        public const string COMMAND_QUIT = "q";
        public const string COMMAND_OK = "ok";
        public const string COMMAND_YES = "y";
        public const string COMMAND_NO = "n";

        #region Filter types
        public const string FILTER_TYPE_DIR = "DIR";
        public const string FILTER_TYPE_FOLDER = "FLD";
        public const string FILTER_TYPE_FILE = "FILE";
        public const string FILTER_TYPE_CLASS = "TYPE";
        public const string FILTER_TYPE_NAMESPACE = "NS";
        public const string FILTER_TYPE_ATTRIBUTE = "ATTR";
        #endregion
        #region Contexts & Commands
        public const string CONTEXT_CFG = "cfg";
        public const string CONTEXT_SYS = "sys";
        public const string CONTEXT_CI = "ci";
        public const string CONTEXT_TARGET = "trg";
        public const string CONTEXT_RUNNER = "runner";
        
        public const string COMMAND_NEW = "new";
        public const string COMMAND_EDIT = "edit";
        public const string COMMAND_DELETE = "del";
        public const string COMMAND_COPY = "copy";
        public const string COMMAND_VIEW = "view";
        public const string COMMAND_LIST = "list";

        public const string COMMAND_CLS = "cls";
        public const string COMMAND_HELP = "?";
        public const string COMMAND_HELP_2 = "help";
        public const string COMMAND_START = "start";
        public const string COMMAND_ACTIVATE = "activate";
        #endregion

        internal const string CONFIG_INJECTOR_MODEL = "injector.yml";
        internal const string CONFIG_TEST_RUNNER_MODEL = "test_runner.yml";

        internal const string MESSAGE_PROPERTIES_EDIT_WARNING = "Please note that only some basic settings can be changed now. You can read and edit the full list of properties in the corresponding configuration files.";
    }
}

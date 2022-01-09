namespace Drill4Net.Configurator
{
    /// <summary>
    /// Constants for Configurator
    /// </summary>
    public static class ConfiguratorConstants
    {
        public const string ANSWER_QUIT = "q";
        public const string ANSWER_OK = "ok";
        public const string ANSWER_YES = "y";
        public const string ANSWER_NO = "n";

        #region Filter types
        public const string FILTER_TYPE_DIR = "DIR";
        public const string FILTER_TYPE_FOLDER = "FLD";
        public const string FILTER_TYPE_FILE = "FILE";
        public const string FILTER_TYPE_CLASS = "TYPE";
        public const string FILTER_TYPE_NAMESPACE = "NS";
        public const string FILTER_TYPE_ATTRIBUTE = "ATTR";
        #endregion
        #region Contexts & Commands
        public const string CONTEXT_ABOUT = "about";
        public const string CONTEXT_CFG = "cfg";
        public const string CONTEXT_SYS = "sys";
        public const string CONTEXT_CI = "ci";
        public const string CONTEXT_TARGET = "trg";
        public const string CONTEXT_RUNNER = "run";

        public const string COMMAND_NEW = "new";
        public const string COMMAND_EDIT = "edit";
        public const string COMMAND_DELETE = "del";
        public const string COMMAND_COPY = "copy";
        public const string COMMAND_VIEW = "view";
        public const string COMMAND_OPEN = "open";
        public const string COMMAND_LIST = "list";
        public const string COMMAND_CHECK = "check";
        public const string COMMAND_PREP = "prep";
        public const string COMMAND_RESTORE = "restore";

        public const string COMMAND_CLS = "cls";
        public const string COMMAND_START = "start";
        public const string COMMAND_ACTIVE = "active";

        public const char SWITCH_INTEGRATION = 'i';
        public const char SWITCH_INTEGRATION_NO = 'I';
        public const char SWITCH_LAST = 'l';
        public const char SWITCH_ACTIVE = 'a';
        public const char SWITCH_DEFAULT = 'd';
        public const char SWITCH_DEFAULT_NO = 'D';
        public const char SWITCH_CONTENT_NO = 'C';
        #endregion
        #region App paths
        public const string PATH_INSTALL = @"..\..\install\";
        public const string PATH_INJECTOR = @"..\..\apps\injector\";
        public const string PATH_RUNNER = @"..\..\apps\test_runner\";
        public const string PATH_CI = @"..\..\ci\";
        public const string PATH_TRANSMITTER = @"..\..\components\transmitter\";
        public const string PATH_TRANSMITTER_PLUGINS = @"..\..\components\transmitter_plugins\"; //Agent's plugins (IEngineContexters)
        #endregion

        internal const string CONFIG_INJECTOR_MODEL = "injector.yml";
        internal const string CONFIG_TEST_RUNNER_MODEL = "test_runner.yml";

        internal const string MESSAGE_PROPERTIES_EDIT_WARNING = "Please note that only some basic settings can be changed now. You can read and edit the full list of properties in the corresponding configuration files.";
        internal const string MESSAGE_CI_INTEGRATION_IDE_DIR = "Specify the directory of one or more projects/solutions with .NET source code";
    }
}

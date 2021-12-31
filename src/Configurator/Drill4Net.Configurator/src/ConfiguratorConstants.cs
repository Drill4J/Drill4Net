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
        public const string FILTER_TYPE_TYPE = "TYPE";
        public const string FILTER_TYPE_NAMESPACE = "NS";
        public const string FILTER_TYPE_ATTRIBUTE = "ATTR";
        #endregion
        #region Commands
        public const string COMMAND_SYS = "sys cfg";
        public const string COMMAND_CI = "ci cfg";
        public const string COMMAND_TARGET = "trg cfg";
        public const string COMMAND_RUNNER = "runner cfg";
        public const string COMMAND_START = "ci start";

        public const string COMMAND_DELETE = "delete";
        public const string COMMAND_ACTIVATE = "activate";

        public const string COMMAND_NEW = "new";
        public const string COMMAND_EDIT = "edit";
        public const string COMMAND_VIEW = "view";
        #endregion

        internal const string CONFIG_INJECTOR_MODEL = "injector.yml";

        internal const string MESSAGE_PROPERTIES_EDIT_WARNING = "Please note that only some basic settings can be changed now. You can read and edit the full list of properties in the corresponding configuration files.";
    }
}

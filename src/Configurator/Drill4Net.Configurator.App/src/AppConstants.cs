namespace Drill4Net.Configurator.App
{
    internal static class AppConstants
    {
        #region Console colors
        internal const ConsoleColor COLOR_INFO = ConsoleColor.Cyan;
        internal const ConsoleColor COLOR_QUESTION = ConsoleColor.Cyan;
        internal const ConsoleColor COLOR_ANSWER = ConsoleColor.Yellow;
        internal const ConsoleColor COLOR_ERROR = ConsoleColor.Red;
        internal const ConsoleColor COLOR_DEFAULT = ConsoleColor.Green;
        internal const ConsoleColor COLOR_TEXT = ConsoleColor.White;
        internal const ConsoleColor COLOR_TEXT_HIGHLITED = ConsoleColor.Yellow;
        internal const ConsoleColor COLOR_TEXT_WARNING = ConsoleColor.DarkYellow;
        #endregion
        #region Commands
        internal const string COMMAND_QUIT = "q";
        internal const string COMMAND_OK = "ok";
        internal const string COMMAND_YES = "y";
        internal const string COMMAND_NO = "n";

        internal const string COMMAND_SYS = "sys";
        internal const string COMMAND_CI = "ci";
        internal const string COMMAND_TARGET = "trg";
        internal const string COMMAND_RUNNER = "runner";

        internal const string COMMAND_DELETE = "delete";
        internal const string COMMAND_ACTIVATE = "activate";

        internal const string COMMAND_NEW = "new";
        internal const string COMMAND_EDIT = "edit";
        internal const string COMMAND_VIEW = "view";
        #endregion
        #region Filter types
        internal const string FILTER_TYPE_DIR = "DIR";
        internal const string FILTER_TYPE_FOLDER = "FLD";
        internal const string FILTER_TYPE_FILE = "FILE";
        internal const string FILTER_TYPE_TYPE = "TYPE";
        internal const string FILTER_TYPE_NAMESPACE = "NS";
        internal const string FILTER_TYPE_ATTRIBUTE = "ATTR";
        #endregion

        internal const string CONFIG_INJECTOR_MODEL = "injector.yml";
        internal const string MESSAGE_PROPERTIES_EDIT_WARNING = "Please note that only some basic settings can be changed now. You can read and edit the full list of properties in the corresponding configuration files.";
    }
}

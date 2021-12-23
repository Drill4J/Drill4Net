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
        internal const string COMMAND_YES = "y";
        internal const string COMMAND_NO = "n";

        internal const string COMMAND_SYS = "sys";
        internal const string COMMAND_CI = "ci";

        internal const string COMMAND_TARGET = "target";
        internal const string COMMAND_TARGET_DELETE = "delete";
        internal const string COMMAND_TARGET_ACTIVATE = "activate";

        internal const string COMMAND_NEW = "new";
        internal const string COMMAND_EDIT = "edit";
        internal const string COMMAND_VIEW = "view";
        #endregion

        internal const string MESSAGE_PROPERTIES_EDIT_WARNING = "Please note that only some basic settings can be changed now. You can read and edit the full list of properties in the corresponding configuration files.";
    }
}

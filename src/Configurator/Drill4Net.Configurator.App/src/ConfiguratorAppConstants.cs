﻿namespace Drill4Net.Configurator.App
{
    internal static class ConfiguratorAppConstants
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
        internal const string COMMAND_SYS = "sys";
        internal const string COMMAND_CI = "ci";

        internal const string COMMAND_TARGET = "target";
        internal const string COMMAND_TARGET_DELETE = "delete";
        internal const string COMMAND_TARGET_ACTIVATE = "activate";

        internal const string COMMAND_QUIT = "q";
        internal const string COMMAND_NEW = "new";
        internal const string COMMAND_EDIT = "edit";
        internal const string COMMAND_VIEW = "view";
        #endregion
    }
}

namespace Drill4Net.Configurator
{
    internal static class HelpHelper
    {
        internal static string GetArgumentsForSourceConfig(string appName, string cmd, string fld, bool showAboutActiveConfig = false)
        {
            return $@"{GetActiveLastSwitchesDesc(appName, cmd)}

{GetExplicitConfigDesc(appName, cmd, fld)}

{GetShortNameConfigNote(appName, showAboutActiveConfig)}";
        }

        internal static string GetActiveLastSwitchesDesc(string appName, string cmd)
        {
            return @$"You can use some switches for implicit specifying the {appName} config: ""a"" for the active one and ""l"" for the last edited one.
    Example: {cmd} -a
    Example: {cmd} -l";
        }

        internal static string GetLastSwitchesDesc(string appName, string cmd)
        {
            return @$"You can use the switch ""l"" (the last edited config) for implicit specifying the {appName} one.
    Example: {cmd} -l";
        }

        internal static string GetExplicitConfigDesc(string appName, string cmd, string fld)
        {
            return $@"Also you can to do it by passing the explicit short name of {appName} config file or its full path as positional parameter:
    Example: {cmd} -- cfg2
    Example: {cmd} -- ""d:\configs\{fld}\cfg2.yml""

...or with named argument:
    Example: {cmd} --cfg_path=""d:\configs\{fld}\cfg2.yml""";
        }

        internal static string GetActiveConfigText(string appName, bool isProgram, string cmd, string fld)
        {
            var str = $"The command sets the specified config active, the link to which is written to the special redirection config _redirect.yml, located in the root of the {appName} folder. The corresponding process will use the specified config as the default config, which makes it possible to run the program without arguments";

            if(isProgram)
                str += ", for example, by clicking on the icon from the file manager.";
            else
                str += ".";

            str += @$" In addition, you can call the rest of the commands in a short form using switch ""-l"".

    Example: {cmd} -l

{GetExplicitConfigDesc(appName, cmd, fld)}

{GetShortNameConfigNote(appName, false)}";

            return str;
        }

        internal static string GetShortNameConfigNote(string appName, bool showAboutActiveConfig)
        {
            var aboutSwitches = showAboutActiveConfig ? @"switches ""l"" and ""a""" : @"switch ""l""";
            return $"Configs should be located in the root directory {appName} if they are specified by a short name (with or without an extension). The same applies to the use of {aboutSwitches}.";
        }

        internal static string GetInjectorAndRunnerConfigSavingNote(string appName)
        {
            return $"You can specify either just the name of the config (with or without an extension), and in this case it will be saved to the standard {appName} folder, or specify the full arbitrary path, for example, for the target folder in the case of CI pipeline.";
        }
    }
}

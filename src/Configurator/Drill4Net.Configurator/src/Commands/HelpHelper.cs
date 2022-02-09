using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    internal static class HelpHelper
    {
        internal static string GetCopyConfigDesc(string appName, string cmd, string folder, string addition = "")
        {
            var addS = string.IsNullOrWhiteSpace(addition) ? null : addition + " ";
            return @$"You should specify source config and destination one. 
{GetActiveLastSwitchesShortDesc(appName)}
You can to pass the explicit short name of {appName} config file or its full path, and you can use they as positional parameters.

  Example: {cmd} --{CoreConstants.ARGUMENT_CONFIG_PATH}=""d:\configs\{folder}\source.yml"" ""d:\Drill4Net\{folder}\destination.yml"" {addS}(source.yml will copied to the destination.yml)
  Example: {cmd} -- ""d:\Drill4Net\{folder}\destination.yml"" -l {addS}(last edited config will copied to the destination.yml)
  Example: {cmd} -- destination.yml -l {addS}(last edited config in the {appName} directory will copied to the destination.yml in the same folder, the position of the switch - before or after the name is not important)
  Example: {cmd} -- ""d:\configs\{folder}\source.yml"" ""d:\Drill4Net\{folder}\destination.yml"" {addS}";
        }

        internal static string GetArgumentsForSourceConfig(string appName, string cmd, string fld, bool showAboutActiveConfig = false)
        {
            return $@"{GetActiveLastSwitchesDesc(appName, cmd)}

{GetPositionalConfigDesc(appName, cmd, fld)}

{GetShortNameConfigNote(appName, showAboutActiveConfig)}";
        }

        internal static string GetActiveLastSwitchesDesc(string appName, string cmd)
        {
            return @$"{GetActiveLastSwitchesShortDesc(appName)}
    Example: {cmd} -a
    Example: {cmd} -l";
        }

        internal static string GetActiveLastSwitchesShortDesc(string appName)
        {
            return @$"You can use some switches for implicit specifying the {appName} config: ""a"" for the active one and ""l"" for the last edited one.";
        }

        internal static string GetLastSwitchesDesc(string appName, string cmd)
        {
            return @$"{GetLastSwitchesShortDesc(appName)}
    Example: {cmd} -l";
        }

        internal static string GetLastSwitchesShortDesc(string appName)
        {
            return @$"You can use the switch ""l"" (the last edited config) for implicit specifying the {appName} one.";
        }

        internal static string GetPositionalConfigDesc(string appName, string cmd, string folder)
        {
            return $@"Also you can to do it by passing the explicit short name of {appName} config file or its full path as positional parameter:
    Example: {cmd} -- cfg2
    Example: {cmd} -- ""d:\configs\{folder}\cfg2.yml""

...or with named argument:
    Example: {cmd} --{CoreConstants.ARGUMENT_CONFIG_PATH}=""d:\configs\{folder}\cfg2.yml""";
        }

        internal static string GetActiveConfigText(string appName, bool isProgram, string cmd, string folder)
        {
            var str = $"The command sets the specified config active, the link to which is written to the special redirection config _redirect.yml, located in the root of the {appName} folder. The corresponding process will use the specified config as the default config, which makes it possible to run the program without arguments";

            if(isProgram)
                str += ", for example, by clicking on the icon from the file manager.";
            else
                str += ".";

            str += @$" In addition, you can call the rest of the commands in a short form using switch ""-l"".

    Example: {cmd} -l

{GetPositionalConfigDesc(appName, cmd, folder)}

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

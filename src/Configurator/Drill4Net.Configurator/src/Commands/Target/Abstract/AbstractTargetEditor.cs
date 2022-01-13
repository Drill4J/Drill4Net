using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    public abstract class AbstractTargetEditor : AbstractConfiguratorCommand
    {
        protected AbstractTargetEditor(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /************************************************************************/

        public bool Edit(string cfgPath, bool isNew)
        {
            _logger.Info($"Start configure the target: new={isNew}");
            var appName = CoreConstants.SUBSYSTEM_INJECTOR;

            #region Config
            if (!File.Exists(cfgPath))
            {
                RaiseError($"{appName} config not found: [{cfgPath}]");
                return false;
            }
            var cfg = _rep.ReadInjectorOptions(cfgPath);
            #endregion
            #region Source dir
            string sourceDir = "";
            string def = isNew ? "" : cfg.Source.Directory;
            do
            {
                if (!_cli.AskQuestion($"Target's directory (compiled assemblies). It can be full or relative (for the {appName} program)",
                    out sourceDir, def, !string.IsNullOrWhiteSpace(def)))
                    return false;
            }
            while (!_cli.CheckDirectoryAnswer(ref sourceDir, true));
            cfg.Source.Directory = sourceDir;
            #endregion
            #region Name
            string name = "";
            def = isNew ? GetDefaultTargetName(sourceDir) : cfg.Target.Name;
            do
            {
                if (!_cli.AskQuestion("Target's name (as Agent ID). It must be the same for the different builds of the Product", out name, def))
                    return false;
            }
            while (!_cli.CheckStringAnswer(ref name, "Target's name cannot be empty", false));
            cfg.Target.Name = name;
            #endregion

            // Description
            def = isNew ? "" : cfg.Description;
            if (!_cli.AskQuestion("Target's description", out string desc, def, !string.IsNullOrWhiteSpace(def)))
                return false;
            cfg.Description = desc;

            if (cfg.Target == null)
                cfg.Target = new();

            #region Version
            def = "1";
            if (!isNew)
            {
                if (!string.IsNullOrWhiteSpace(cfg.Target.Version))
                    def = "1";
                else
                if (!string.IsNullOrWhiteSpace(cfg.Target.VersionAssemblyName))
                    def = "2";
            }
            string answer;
            const string versionQuestion = @"Type of retrieving the target build's version:
  1. I set the specific version now. It will be stored in metadata file for this target's build during the injection.
  2. The version will be retrieved from compiled assembly, I will specify the file name. It will be stored in metadata file, too.
  3. The version will be passed through the CI pipeline using the Test Runner argument (in the case of autotests).

Please make your choice";
            while (true)
            {
                if (!_cli.AskQuestion(versionQuestion, out answer, def))
                    return false;
                if (!_cli.CheckIntegerAnswer(answer, "Please select from 1 to 3", 1, 3, out int choice))
                    continue;
                //
                switch (choice)
                {
                    case 1:
                        var version = isNew ? "" : cfg.Target.Version;
                        if (!AskTargetVersonFromUser(ref version))
                            return false;
                        cfg.Target.Version = version;
                        break;
                    case 2:
                        var asmName = isNew ? "" : cfg.Target.VersionAssemblyName;
                        if (!AskVersionAssemblyName(ref asmName))
                            return false;
                        cfg.Target.VersionAssemblyName = asmName;
                        break;
                    case 3:
                        var mess = "The target's version won't stored in tree file (metadata of target's injection) - so, if the version will be missed ALSO in Agent config (it is one more possibility to pass this value to the Drill admin side), CI engineer is responsible for passing the actual one to the Test Runner by argument.";
                        RaiseWarning(mess);
                        break;
                    default:
                        continue;
                }
                break;
            }
            #endregion
            #region Filter
            const string filterMess = "\nNow you need to set up some rules for injected entities: folders, files, namespaces, classes, etc.\n";
            RaiseMessage(filterMess, CliMessageType.Annotation);

            const string filterHint = $@"We should process only the Target's ones. The system files the Injector can skip on its own (as a rule), but in the case of third-party libraries, this may be too difficult to do automatically. 
If the target contains one shared root namespace (e.g. Drill4Net at beginning of Drill4Net.BanderLog), the better choice is to use the rules for type ""Namespace"" (in this case value is ""Drill4Net"").
Hint: the values can be just strings or regular expressions (e.g. ""{CoreConstants.REGEX_FILTER_PREFIX} ^Drill4Net\.([\w-]+\.)+[\w]*Tests$"").
Hint: the test frameworks' assemblies (that do not contains user functionality) should be skipped.
For more information, please read documentation.

You can enter several rules in separate strings, e.g. first for files, then for classes, and so on. Separate several rules with a semicolon.
Separate several entities in one rule with a comma. 
You can use Include and Exclude rules at the same time. By default, a rule has Include type.

To finish, just enter ""{ConfiguratorConstants.ANSWER_OK}"".

The filters:
  - By directory (full path). Examples:
       {ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\ - Include rule by default
       {ConfiguratorConstants.FILTER_TYPE_DIR}=^d:\Projects\ABC\ - Exclude rule (note the ^ sign at the beginning of the value)
       {ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\,d:\Projects\123\ (two directories in one type's tag with comma)
       {ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\;{ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\123\ (two directories in separate blocks with semicolon)
  - By folder (just name). Example: 
       {ConfiguratorConstants.FILTER_TYPE_FOLDER}: ref
  - By file (short name). Example: 
       {ConfiguratorConstants.FILTER_TYPE_FILE}: Drill4Net.Target.Common.dll
  - By namespace (by beginning if value is presented as string, or by regex). Examples:
       {ConfiguratorConstants.FILTER_TYPE_NAMESPACE}: Drill4Net
       {ConfiguratorConstants.FILTER_TYPE_NAMESPACE}: reg: ^Drill4Net\.([\w-]+\.)+[\w]*Tests$
  - By class fullname (with namespace). Example:
       {ConfiguratorConstants.FILTER_TYPE_CLASS}: Drill4Net.Target.Common.ModelTarget
  - By attribute of type. Example:
       {ConfiguratorConstants.FILTER_TYPE_ATTRIBUTE}: Drill4Net.Target.SuperAttribute (it is full name of its type)

Hint: the regex must be located only in a sole filter tag (one expression in one tag).
Hint: to set up value for ""all entities of current filter type"" use sign *. Example:
       {ConfiguratorConstants.FILTER_TYPE_FOLDER}: ^* (do not process any folders)";
            RaiseMessage(filterHint, CliMessageType.Help);

            var filterQuestion = "Please create at least one filter rule";
            RaiseQuestion($"\n{filterQuestion}: ");

            while (true)
            {
                answer = Console.ReadLine().Trim().ToLower();
                if (_cli.IsOk(answer))
                    break;
                if (!AddFilterRules(answer, cfg.Source.Filter, out string err))
                    RaiseError(err);
            }
            #endregion
            #region Destination dir
            def = "1";
            if (!isNew)
            {
                if (!string.IsNullOrWhiteSpace(cfg.Destination.FolderPostfix))
                    def = "1";
                else
                    def = "2";
            }
            const string destQuestion = @"Configure destination directory for the instrumented target by:
  1. Just postfix for the original target's directory, which will be located after it and the point. The folders will be located side by side.
  2. Arbitrary path to the processed folder. It can be full or relative (for the Injector program).

Please make your choice";
            while (true)
            {
                if (!_cli.AskQuestion(destQuestion, out answer, def))
                    return false;
                if (!_cli.CheckIntegerAnswer(answer, "Please select from 1 to 2", 1, 2, out int choice))
                    continue;
                //
                switch (choice)
                {
                    case 1:
                        var postfix = isNew ? "" : cfg.Destination.FolderPostfix;
                        if (!AskDestinationPostfix(ref postfix))
                            return false;
                        cfg.Destination.FolderPostfix = postfix;
                        break;
                    case 2:
                        if (!_cli.AskDirectory("Destination's directory (processed assemblies). It may not exist yet", out var destDir, null, false, false))
                            return false;
                        cfg.Destination.Directory = destDir;
                        break;
                    default:
                        continue;
                }
                break;
            }
            #endregion

            // Logs
            if (cfg.Logs == null)
                cfg.Logs = new();
            if (!_cli.AddLogFile(cfg.Logs, appName))
                return false;

            // save config
            if (isNew)
                return _cmdHelper.AskNameAndSave(appName, cfg, _rep.GetInjectorDirectory(), true);
            else
                return _cmdHelper.SaveConfig(appName, cfg, cfgPath);
        }

        private bool AskDestinationPostfix(ref string postfix)
        {
            var def = string.IsNullOrWhiteSpace(postfix) ? "Injected" : postfix;
            do
            {
                if (!_cli.AskQuestion("Postfix to original directory", out postfix, def))
                    return false;
            }
            while (!_cli.CheckStringAnswer(ref postfix, "Postfix cannot be empty."));
            return true;
        }

        private bool AskVersionAssemblyName(ref string name)
        {
            var def = string.IsNullOrWhiteSpace(name) ? "" : name;
            do
            {
                if (!_cli.AskQuestion("Set the assembly name (dll) with extension which contains the actual Product's version",
                    out name, def, !string.IsNullOrWhiteSpace(def)))
                    return false;
            }
            while (!_cli.CheckFileNameAnswer(ref name, "The assembly name cannot be empty", false));
            return true;
        }

        internal bool AskTargetVersonFromUser(ref string version)
        {
            var def = string.IsNullOrWhiteSpace(version) ? null : version;
            while (true)
            {
                if (!_cli.AskQuestion("Target's version (SemVer format)", out version, def, !string.IsNullOrWhiteSpace(def)))
                    return false;
                //version = "0.8.240"; // "0.8.240-main+2befca57f1"
                var pattern = new Regex(@"\d+(\.\d+)+([-](\p{L})+)?([+]([0-9a-zA-Z])+)?");
                var isMatch = pattern.IsMatch(version);
                if (isMatch)
                    return true;
                RaiseWarning("The version format is incorrect. It must be something like that: 0.8.240, or 0.8.240-main, or even 0.8.240-main+2befca57f1");
            }
        }

        internal string GetDefaultTargetName(string sourceDir)
        {
            if (string.IsNullOrWhiteSpace(sourceDir))
                throw new ArgumentNullException(nameof(sourceDir));
            var name = ReplaceSpecialSymbolsForTargetName(new DirectoryInfo(sourceDir).Name);

            //split name by "-" for Camel notation
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var ch = name[i];
                if (char.IsUpper(ch) && i > 0 && name[i - 1] != '-')
                    sb.Append('-');
                sb.Append(ch);
            }
            return sb.ToString().ToLower();
        }

        internal string ReplaceSpecialSymbolsForTargetName(string name)
        {
            name = name.Replace(" ", "-");
            name = Regex.Replace(name, @"[^\w\d]", "-");
            while (name.Contains("--")) //Guanito - TODO: regex
                name = name.Replace("--", "-");
            return name;
        }

        internal bool AddFilterRules(string input, SourceFilterOptions filter, out string error)
        {
            error = "";
            #region Check
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Rule cannot be empty";
                return false;
            }
            input = input.Trim();
            #endregion

            //rules
            List<string> rules;
            if (input.Contains(';'))
                rules = input.Split(";").ToList();
            else
                rules = new() { input };
            //
            foreach (var rule in rules)
            {
                if (!rule.Contains('='))
                {
                    error = "Rule must contain the sign = between its type and value";
                    return false;
                }
                var ar = rule.Split("=");
                var type = ar[0];

                //get rules' values
                var val = ar[1];
                List<string> vals;
                if (val.Contains(','))
                    vals = val.Split(",").ToList();
                else
                    vals = new() { val };

                // add the rules' values
                foreach (var curVal in vals)
                {
                    var isExclude = curVal.StartsWith("^");
                    SourceFilterParams pars = isExclude ? filter.Excludes : filter.Includes;
                    var actualVal = (isExclude ? curVal[1..] : curVal)?.Trim();
                    switch (type.ToUpper())
                    {
                        case ConfiguratorConstants.FILTER_TYPE_DIR:
                            pars.AddDirectory(actualVal);
                            break;
                        case ConfiguratorConstants.FILTER_TYPE_FOLDER:
                            pars.AddFolder(actualVal);
                            break;
                        case ConfiguratorConstants.FILTER_TYPE_FILE:
                            pars.AddFile(actualVal);
                            break;
                        case ConfiguratorConstants.FILTER_TYPE_NAMESPACE:
                            pars.AddNamespace(actualVal);
                            break;
                        case ConfiguratorConstants.FILTER_TYPE_CLASS:
                            pars.AddClass(actualVal);
                            break;
                        case ConfiguratorConstants.FILTER_TYPE_ATTRIBUTE:
                            pars.AddAttribute(actualVal);
                            break;
                        default:
                            error = $"Unknown filter type: {type}";
                            return false;
                    }
                }
            }
            return true;
        }
    }
}

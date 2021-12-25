﻿using System;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Configuration;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator.App
{
    internal class InputProcessor
    {
        private readonly ConfiguratorRepository _rep;
        private readonly ConfiguratorOutputHelper _outputHelper;
        private BaseOptionsHelper _optHelper;
        private readonly Logger _logger;

        /**********************************************************************/

        public InputProcessor(ConfiguratorRepository rep, ConfiguratorOutputHelper outputHelper)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            _logger = new TypedLogger<InputProcessor>(rep.Subsystem);
            _optHelper = new(_rep.Subsystem);
        }

        /**********************************************************************/

        public void Start(string[] args)
        {
            //_outputHelper.WriteMessage("Configurator is initializing...", ConfiguratorAppConstants.COLOR_TEXT);
            if (args == null || args.Length == 0) //interactive poller
            {
                _logger.Info("Interactive mode");
                _outputHelper.PrintMenu();
                StartInteractive();
            }
            else //automatic processing by arguments
            {
                _logger.Info("Automatic mode");
                ProcessByArguments(args);
            }
        }

        internal void ProcessByArguments(string[] args)
        {

        }

        internal void StartInteractive()
        {
            while (true)
            {
                _outputHelper.WriteLine("\nCommand:", AppConstants.COLOR_TEXT);
                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (string.Equals(input, AppConstants.COMMAND_QUIT, StringComparison.OrdinalIgnoreCase))
                    return;
                try
                {
                    ProcessCommand(input);
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"error -> {ex.Message}", AppConstants.COLOR_ERROR);
                }
            }
        }

        private bool ProcessCommand(string input)
        {
            input = input.Trim();
            return input switch
            {
                "?" or "help" => _outputHelper.PrintMenu(),
                AppConstants.COMMAND_SYS => SystemConfigure(),
                AppConstants.COMMAND_TARGET => TargetConfigure(_rep.Options),
                AppConstants.COMMAND_CI => CIConfigure(),
                _ => _outputHelper.PrintMenu(),
            };
        }

        #region System
        internal bool SystemConfigure()
        {
            SystemConfiguration cfg = new();
            if (!ConfigAdmin(_rep.Options, cfg))
                return false;
            if(!ConfigMiddleware(_rep.Options, cfg))
                return false;

            //TODO: view list of all properties

            //need to save?
            _outputHelper.WriteLine("\nSave the system configuration? [y]:", AppConstants.COLOR_QUESTION);
            var answer = Console.ReadLine()?.Trim();
            var yes = IsYes(answer);
            if (yes)
            {
                _outputHelper.Write("YES", true, AppConstants.COLOR_DEFAULT);
                _rep.SaveSystemConfiguration(cfg);
                _outputHelper.WriteLine($"System options are saved. {AppConstants.MESSAGE_PROPERTIES_EDIT_WARNING}",
                    AppConstants.COLOR_TEXT);
            }
            else
            {
                _outputHelper.Write("NO", true, AppConstants.COLOR_TEXT_WARNING);
            }
            return yes;
        }

        internal bool ConfigAdmin(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            //Drill host
            string host = null;
            var def = opts.AdminHost;
            do
            {
                if (!AskQuestion("Drill service host", out host, def))
                    return false;
            }
            while (!CheckStringAnswer(ref host, "The service host address cannot be empty"));
            
            // Drill port
            int port;
            def = opts.AdminPort.ToString();
            string portS;
            do
            {
                if (!AskQuestion("Drill service port", out portS, def))
                    return false;
            }
            while (!CheckIntegerAnswer(portS, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.AdminUrl = $"{host}:{port}";
            _logger.Info($"Admin url: {cfg.AdminUrl }");
            
            // agent's plugin dir
            string plugDir = null;
            def = opts.PluginDirectory;
            do
            {
                if (!AskQuestion("Agent plugin directory", out plugDir, def))
                    return false;
            }
            while (!CheckDirectoryAnswer(ref plugDir, true));
            cfg.AgentPluginDirectory = plugDir;
            _logger.Info($"Plugin dir: {plugDir}");
            //
            return true;
        }

        internal bool ConfigMiddleware(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            // Kafka host
            string host = null;
            var def = opts.MiddlewareHost;
            do
            {
                if (!AskQuestion("Kafka host", out host, def))
                    return false;
            }
            while (!CheckStringAnswer(ref host, "The Kafka host address cannot be empty"));
            
            // Kafka port
            int port;
            def = opts.MiddlewarePort.ToString();
            string portS;
            do
            {
                if (!AskQuestion("Kafka port", out portS, def))
                    return false;
            }
            while (!CheckIntegerAnswer(portS, "The Kafka port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.MiddlewareUrl = $"{host}:{port}";
            _logger.Info($"Kafka url: {cfg.MiddlewareUrl}");

            // Logs
            if (!AddLogFile(cfg.Logs, "Drill system"))
                return false;

            return true;
        }
        #endregion
        #region Target
        internal bool TargetConfigure(ConfiguratorOptions opts)
        {
            #region Init Injector config
            var modelCfgPath = Path.Combine(_rep.Options.InstallDirectory, AppConstants.CONFIG_INJECTOR_MODEL);
            if (!File.Exists(modelCfgPath))
            {
                _outputHelper.WriteLine($"Model Injector's config not found: [{modelCfgPath}]", AppConstants.COLOR_ERROR);
                return false;
            }
            var cfg = _rep.ReadInjectorOptions(modelCfgPath);
            #endregion
            #region Source dir
            string sourceDir = null;
            string def = null;
            do
            {
                if (!AskQuestion("Target's directory (compiled assemblies). It can be full or relative (for the Injector program)",
                    out sourceDir, def, false))
                    return false;
            }
            while (!CheckDirectoryAnswer(ref sourceDir, true));
            cfg.Source.Directory = sourceDir;
            #endregion
            #region Name
            string name = null;
            def = GetDefaultTargetName(sourceDir);
            do
            {
                if (!AskQuestion("Target's name (as Agent ID). It must be the same for the Product and its different builds", out name, def))
                    return false;
            }
            while (!CheckStringAnswer(ref name, "Target's name cannot be empty", false));
            cfg.Target.Name = name;
            #endregion

            // Description
            if (!AskQuestion("Target's description", out string desc, null))
                return false;
            cfg.Description = desc;

            #region Version
            def = "1";
            string answer;
            const string versionQuestion = @"Type of retrieving the target build's version:
  1. I set the specific version now. It will be stored in metadata file for this target's build during the injection.
  2. The version will be retrieved from compiled assembly, I will specify the file name. It will be stored in metadata file, too.
  3. The version will be passed through the CI pipeline using the Test Runner argument (in the case of autotests).

Please make your choice";
            while (true)
            {
                if (!AskQuestion(versionQuestion, out answer, def))
                    return false;
                if(!CheckIntegerAnswer(answer, "Please select from 1 to 3", 1, 3, out int choice))
                    continue;
                //
                switch (choice)
                {
                    case 1:
                        if (!AskTargetVersonFromUser(out string version))
                            return false;
                        cfg.Target.Version = version;
                        break;
                    case 2:
                        if(!AskVersionAssemblyName(out var asmName))
                            return false;
                        cfg.Target.VersionAssemblyName = asmName;
                        break;
                    case 3:
                        var mess = "The target's version won't stored in tree file (metadata of target's injection) - so, if the version will be missed ALSO in Agent's config (it is one more possibility to pass this value to the Drill admin side), CI engineer is responsible for passing the actual one to the Test Runner by argument.";
                        _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
                        break;
                    default:
                        continue;
                }
                break;
            }
            #endregion
            #region Filter
            const string filterQuestion = $@"Now you need to set up some rules for injected entities: files, namespaces, classes, folders, etc.
We should process only the Target's ones. The system files the Injector can skip on its own (as a rule), but in the case of third-party libraries, this may be too difficult to do automatically. 
If the target contains one shared root namespace (e.g. Drill4Net at beginning of Drill4Net.BanderLog), the better choice is to use the rules for type ""Namespace"" (in this case value is ""Drill4Net"").
Hint: the values can be just strings or regular expressions (e.g. ""{CoreConstants.REGEX_FILTER_PREFIX} ^Drill4Net\.([\w-]+\.)+[\w]*Tests$"").
Hint: the test frameworks' assemblies (that do not contains user functionality) should be skipped.
For more information, please read documentation.

You can enter several rules in separate strings, e.g. first for files, then for classes, and so on. Separate several rules with a semicolon.
Separate several entities in one rule with a comma. 
You can use Include and Exclude rules at the same time. By default, a rule has Include type.

To finish, just enter ""ok"".

The filters:
  - By directory (full path). Examples:
       {AppConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\ - Include rule by default
       {AppConstants.FILTER_TYPE_DIR}=^d:\Projects\ABC\ - Exclude rule (note the ^ sign at the beginning of the value)
       {AppConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\,d:\Projects\123\ (two directories in one type's tag with comma)
       {AppConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\;{AppConstants.FILTER_TYPE_DIR}=d:\Projects\123\ (two directories in separate blocks with semicolon)
  - By folder (just name). Example: 
       {AppConstants.FILTER_TYPE_FOLDER}: ref
  - By file (short name). Example: 
       {AppConstants.FILTER_TYPE_FILE}: Drill4Net.Target.Common.dll
  - By namespace (by beginning if value is presented as string, or by regex). Examples:
       {AppConstants.FILTER_TYPE_NAMESPACE}: Drill4Net
       {AppConstants.FILTER_TYPE_NAMESPACE}: reg: ^Drill4Net\.([\w-]+\.)+[\w]*Tests$
  - By class fullname (with namespace). Example:
       {AppConstants.FILTER_TYPE_TYPE}: Drill4Net.Target.Common.ModelTarget
  - By attribute of type. Example:
       {AppConstants.FILTER_TYPE_ATTRIBUTE}: Drill4Net.Target.SuperAttribute (it is full name of its type)

Hint: the regex must be located only in a sole filter tag (one expression in one tag).
Hint: to set up value for ""all entities of current filter type"" use sign *. Example:
       {AppConstants.FILTER_TYPE_FOLDER}: ^* (do not process any folders)

Please create at least one filter rule";
            _outputHelper.WriteLine($"\n{filterQuestion}: ", AppConstants.COLOR_QUESTION);

            while (true)
            {
                answer = Console.ReadLine()?.Trim()?.ToLower();
                if (answer.Replace("\"", null) == "ok")
                    break;
                if(!AddFilterRules(answer, cfg.Source.Filter, out string err))
                    _outputHelper.WriteLine(err, AppConstants.COLOR_ERROR);
            }
            #endregion
            #region Destination dir
            def = "1";
            const string destQuestion = @"Configure destination directory for the instrumented target by:
  1. Just postfix for the original target's directory, which will be located after it and the point. The folders will be located side by side.
  2. Arbitrary path to the processed folder. It can be full or relative (for the Injector program).

Please make your choice";
            while (true)
            {
                if (!AskQuestion(destQuestion, out answer, def))
                    return false;
                if (!CheckIntegerAnswer(answer, "Please select from 1 to 2", 1, 2, out int choice))
                    continue;
                //
                switch (choice)
                {
                    case 1:
                        if (!AskDestinationPostfix(out var postfix))
                            return false;
                        cfg.Destination.FolderPostfix = postfix;
                        break;
                    case 2:
                        if(!AskDirectory("Destination's directory (processed assemblies). It may not exist yet", out var destDir, null, false, false))
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
            if (!AddLogFile(cfg.Logs, CoreConstants.SUBSYSTEM_INJECTOR))
                return false;

            #region Save config
            var injDir = opts.InjectorDirectory;
            if (string.IsNullOrEmpty(injDir))
                injDir = @"..\injector";

            string injCfgPath;
            var needSave = true;
            while (true)
            {
                if (!AskQuestion("Name of the Injector's config", out name, CoreConstants.CONFIG_NAME_DEFAULT))
                    return false;
                if (!CheckFileNameAnswer(ref name, "Wrong file name", false))
                    continue;
                if (!Path.HasExtension(name))
                    name += ".yml";
                injCfgPath = Path.Combine(injDir, name);

                if (File.Exists(injCfgPath))
                {
                    if (!AskQuestion("Such name already exists. Replace?", out answer, "n"))
                        return false;
                    needSave = IsYes(answer);
                }
                break;
            }
            //
            if(needSave)
            {
                _rep.WriteInjectorOptions(cfg, injCfgPath);
                _outputHelper.WriteLine($"You can check the Injector's settings: {injCfgPath}", AppConstants.COLOR_TEXT);

                // activating the config
                (var needActivate, var redirectCfgPath) = IsNeedAcivateConfigFor(injDir, injCfgPath);
                if (needActivate)
                    SaveRedirectFile(injCfgPath, redirectCfgPath);
            }
            #endregion

            return true;
        }

        internal (bool, string) IsNeedAcivateConfigFor(string appDir, string curCfgPath)
        {
            var redirectCfgPath = _optHelper.CreateRedirectConfigPath(appDir);
            var name = Path.GetFileName(curCfgPath);
            var isDefName = name.Equals(CoreConstants.CONFIG_NAME_DEFAULT, StringComparison.InvariantCultureIgnoreCase);
            bool needActivate;
            if (File.Exists(redirectCfgPath))
            {
                var redirData = _optHelper.ReadRedirectData(redirectCfgPath);
                if (redirData == null)
                {
                    needActivate = true;
                }
                else
                {
                    var actualPath = redirData.Path;
                    needActivate = string.IsNullOrWhiteSpace(actualPath) ||
                        !actualPath.Equals(curCfgPath, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            else //no redirect-file
            {
                needActivate = !isDefName;
            }
            return (needActivate, redirectCfgPath);
        }

        internal void SaveRedirectFile(string actualPath, string redirectCfgPath)
        {
            _optHelper.WriteRedirectData(new RedirectData { Path = actualPath }, redirectCfgPath);
        }

        private bool AskDestinationPostfix(out string postfix)
        {
            var def = "Injected";
            do
            {
                if (!AskQuestion("Postfix to original directory", out postfix, def))
                    return false;
            }
            while (!CheckStringAnswer(ref postfix, "Postfix cannot be empty."));
            return true;
        }

        private bool AskVersionAssemblyName(out string name)
        {
            name = null;
            do
            {
                if (!AskQuestion("Set the assembly name (dll) with extension which contains the actual Product's version", out name, null, false))
                    return false;
            }
            while (!CheckFileNameAnswer(ref name, "The assembly name cannot be empty", false));
            return true;
        }

        internal bool AskTargetVersonFromUser(out string version)
        {
            while (true)
            {
                if (!AskQuestion("Target's version (SemVer format)", out version, null, false))
                    return false;
                //version = "0.8.240"; // "0.8.240-main+2befca57f1"
                var pattern = new Regex(@"\d+(\.\d+)+([-](\p{L})+)?([+]([0-9a-zA-Z])+)?");
                var isMatch = pattern.IsMatch(version);
                if (isMatch)
                    return true;
                _outputHelper.WriteLine("The version format is incorrect. It must be something like that: 0.8.240, or 0.8.240-main, or even 0.8.240-main+2befca57f1",
                    AppConstants.COLOR_TEXT_WARNING);
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
                if (char.IsUpper(ch) && i > 0 && name[i-1] != '-')
                    sb.Append('-');
                sb.Append(ch);
            }
            return sb.ToString().ToLower();
        }

        internal string ReplaceSpecialSymbolsForTargetName(string name)
        {
            name = name.Replace(" ", "-");
            name = Regex.Replace(name, @"[^\w\d]", "-");
            while(name.Contains("--")) //Guanito - TODO: regex
                name = name.Replace("--", "-");
            return name;
        }

        internal bool AddFilterRules(string input, SourceFilterOptions filter, out string error)
        {
            error = null;
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
                    var isExclude = curVal?.StartsWith("^") == true;
                    SourceFilterParams pars = isExclude ? filter.Excludes : filter.Includes;
                    var actualVal = (isExclude ? curVal[1..] : curVal)?.Trim();
                    switch (type.ToUpper())
                    {
                        case AppConstants.FILTER_TYPE_DIR:
                            (pars.Directories ??= new()).Add(actualVal);
                            break;
                        case AppConstants.FILTER_TYPE_FOLDER:
                            (pars.Folders ??= new()).Add(actualVal);
                            break;
                        case AppConstants.FILTER_TYPE_FILE:
                            (pars.Files ??= new()).Add(actualVal);
                            break;
                        case AppConstants.FILTER_TYPE_NAMESPACE:
                            (pars.Namespaces ??= new()).Add(actualVal);
                            break;
                        case AppConstants.FILTER_TYPE_TYPE:
                            (pars.Classes ??= new()).Add(actualVal);
                            break;
                        case AppConstants.FILTER_TYPE_ATTRIBUTE:
                            (pars.Attributes ??= new()).Add(actualVal);
                            break;
                        default:
                            error = $"Unknown filter type: {type}";
                            return false;
                    }
                }
                //TODO: remove duplicates
            }
            return true;
        }
        #endregion
        #region CI
        internal bool CIConfigure()
        {
            _outputHelper.WriteLine("\nSorry, CI operations don't implemented yet", AppConstants.COLOR_TEXT);
            return false;
        }
        #endregion
        #region Common
        /// <summary>
        /// Ask the question and get the value.
        /// </summary>
        /// <param name="question">The question for user</param>
        /// <param name="answer">The answer with default, if empty input is getted</param>
        /// <param name="defValue">The default value</param>
        /// <param name="showDefVal">Do it need to output the default value</param>
        /// <returns>False, if user want to quit from the current setup</returns>
        private bool AskQuestion(string question, out string answer, string defValue, bool showDefVal = true)
        {
            if (string.IsNullOrWhiteSpace(question))
                question = "?";
            if (question.EndsWith(":"))
                question = question[1..];
            question = $"\n{question.Trim()}";
            if (showDefVal)
                question = $"{question}: [{defValue}]";
            else
                question += ":";
            //
            _outputHelper.WriteLine(question, AppConstants.COLOR_QUESTION);
            answer = Console.ReadLine()?.Trim();
            if (IsQuit(answer))
                return false;
            //
            var empty = answer?.Length == 0;
            if (empty)
            {
                answer = defValue;
                _outputHelper.Write(answer, true, AppConstants.COLOR_ANSWER);
            }
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return true;
        }

        private bool AskDirectory(string question, out string destDir, string defValue, bool mustExists, bool showDefVal = true)
        {
            destDir = null;
            do
            {
                if (!AskQuestion(question, out destDir, defValue, showDefVal))
                    return false;
            }
            while (!CheckDirectoryAnswer(ref destDir, mustExists));
            return true;
        }

        private bool AskFilePath(string question, out string filePath, string defValue, bool mustExists, bool showDefVal = true)
        {
            while(true)
            {
                if (!AskQuestion(question, out filePath, defValue, showDefVal))
                    return false;
                string answer = filePath;
                if (!CheckStringAnswer(ref answer, "File path cannot be empty", true))
                    continue;
                var dir = Path.GetDirectoryName(filePath);
                if (!CheckDirectoryAnswer(ref dir, mustExists))
                    continue;
                var fileName = Path.GetFileName(filePath);
                if (!CheckFileNameAnswer(ref fileName, null, !mustExists))
                    continue;
                break;
            }
            return true;
        }

        /// <summary>
        /// Add file log options to the config.
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="programName">Name of program</param>
        /// <returns>If false, it is the need to exit from this setup.</returns>
        private bool AddLogFile(List<LogData> logs, string programName = "program")
        {
            if (!AskQuestion($"The {programName} logs will be output to the its console and to a file in the its {LoggerHelper.LOG_FOLDER} folder. Add an additional parallel log file?", out var answer, "n"))
                return false;
            if (IsYes(answer))
            {
                if (!AskFilePath("File path", out var logPath, null, false, false))
                    return false;
                //
                var logLevel = LogLevel.Debug;
                while (true)
                {
                    //log level
                    if (!AskQuestion("Set the log level", out var logTypeS, logLevel.ToString()))
                        return false;
                    logTypeS = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(logTypeS);
                    if (Enum.TryParse(typeof(LogLevel), logTypeS, out object logType))
                    {
                        logLevel = (LogLevel)logType;
                        break;
                    }
                    _outputHelper.WriteLine($"Unknown type of log level: {logTypeS}", AppConstants.COLOR_TEXT_WARNING);
                }
                //
                var logData = new LogData()
                {
                    Disabled = false,
                    Type = LogSinkType.File,
                    Path = logPath,
                    Level = logLevel,
                };
                logs.Add(logData);
            }
            return true;
        }

        private bool CheckStringAnswer(ref string answer, string mess, bool canBeEmpty = false)
        {
            if (canBeEmpty || !string.IsNullOrWhiteSpace(answer))
                return true;
            _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckIntegerAnswer(string answer, string mess, int min, int max, out int val)
        {
            if (int.TryParse(answer, out val) && val >= min && val <= max)
                return true;
            _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckDirectoryAnswer(ref string directory, bool mustExist = true)
        {
            if (mustExist && string.IsNullOrWhiteSpace(directory))
            {
                _outputHelper.Write("Directory cannot be empty and must exist", true, AppConstants.COLOR_TEXT_WARNING);
                return false;
            }
            if (!mustExist || (mustExist && Directory.Exists(directory)))
            {
                if (directory?.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    _outputHelper.WriteLine("The directory is invalid.", AppConstants.COLOR_TEXT_WARNING);
                    return false;
                }

                //TODO: check for proper dir path itself (cross-platform!)
                return true;
            }
            _outputHelper.WriteLine("Directory does not exists.", AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckFileNameAnswer(ref string filename, string mess, bool canBeEmpty)
        {
            if (!canBeEmpty && string.IsNullOrWhiteSpace(filename))
            {
                _outputHelper.Write("Filename cannot be empty", true, AppConstants.COLOR_TEXT_WARNING);
                return false;
            }
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                if (string.IsNullOrWhiteSpace(mess))
                    mess = "File name contains invalid symbols";
                _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
                return false;
            }
            return true;
        }

        private bool IsQuit(string s)
        {
            return string.Equals(s, AppConstants.COMMAND_QUIT, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsYes(string s, bool noInputIsYes = true)
        {
            if (s == "" && noInputIsYes)
                return true;
            return string.Equals(s, AppConstants.COMMAND_YES, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
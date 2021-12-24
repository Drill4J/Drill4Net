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
        private readonly Logger _logger;

        /**********************************************************************/

        public InputProcessor(ConfiguratorRepository rep, ConfiguratorOutputHelper outputHelper)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            _logger = new TypedLogger<InputProcessor>(rep.Subsystem);
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
                AppConstants.COMMAND_TARGET => TargetConfigure(),
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
                if (IsQuit(host))
                    return false;
                host = AskQuestion("Drill service host", def);
            }
            while (!CheckStringAnswer(ref host, def, "The service host address cannot be empty"));
            
            // Drill port
            int port;
            def = opts.AdminPort.ToString();
            string portS = null;
            do
            {
                if (IsQuit(portS))
                    return false;
                portS = AskQuestion("Drill service port", def);
            }
            while (!CheckIntegerAnswer(portS, def, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.AdminUrl = $"{host}:{port}";
            _logger.Info($"Admin url: {cfg.AdminUrl }");
            
            // agent's plugin dir
            string plugDir = null;
            def = opts.PluginDirectory;
            do
            {
                if (IsQuit(plugDir))
                    return false;
                plugDir = AskQuestion("Agent plugin directory", def);
            }
            while (!CheckDirectoryAnswer(ref plugDir, def, true));
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
                if(IsQuit(host))
                    return false;
                host = AskQuestion("Kafka host", def);
            }
            while (!CheckStringAnswer(ref host, def, "The Kafka host address cannot be empty"));
            
            // Kafka port
            int port;
            def = opts.MiddlewarePort.ToString();
            string portS = null;
            do
            {
                if (IsQuit(portS))
                    return false;
                portS = AskQuestion("Kafka port", def);
            }
            while (!CheckIntegerAnswer(portS, def, "The Kafka port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.MiddlewareUrl = $"{host}:{port}";
            _logger.Info($"Kafka url: {cfg.MiddlewareUrl}");
            return true;
        }
        #endregion
        #region Target
        internal bool TargetConfigure()
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
                if (IsQuit(sourceDir))
                    return false;
                sourceDir = AskQuestion("Target's directory (compiled assemblies). It can be full or relative (for the Injector program)", def, false);
            }
            while (!CheckDirectoryAnswer(ref sourceDir, def, true));
            cfg.Source.Directory = sourceDir;
            #endregion
            #region Name
            string name = null;
            def = GetDefaultTargetName(sourceDir);
            do
            {
                if (IsQuit(name))
                    return false;
                name = AskQuestion("Target's name (as Agent ID). It must be the same for the Product and its different builds", def);
            }
            while (!CheckStringAnswer(ref name, def, "Target's name cannot be empty", false));
            cfg.Target.Name = name;
            #endregion

            // Description
            cfg.Description = AskQuestion("Target's description", null);

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
                answer = AskQuestion(versionQuestion, def);
                if (IsQuit(answer))
                    return false;
                if(!CheckIntegerAnswer(answer, def, "Please select from 1 to 3", 1, 3, out int choice))
                    continue;
                //
                switch (choice)
                {
                    case 1:
                        cfg.Target.Version = AskTargetVersonFromUser();
                        break;
                    case 2:
                        cfg.Target.VersionAssemblyName = AskVersionAssemblyName();
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
                answer = AskQuestion(destQuestion, def);
                if (IsQuit(answer))
                    return false;
                if (!CheckIntegerAnswer(answer, def, "Please select from 1 to 2", 1, 2, out int choice))
                    continue;
                //
                switch (choice)
                {
                    case 1:
                        var postfix = AskDestinationPostfix();
                        if (IsQuit(postfix))
                            return false;
                        cfg.Destination.FolderPostfix = postfix;
                        break;
                    case 2:
                        var destDir = AskDirectory("Destination's directory (processed assemblies). It may not exist yet", null, false, false);
                        if (IsQuit(destDir))
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
            if (!AddLogFile(cfg, "Injector"))
                return false;
            //

            //
            return true;
        }

        private string AskDestinationPostfix()
        {
            string postfix = null;
            string def = "Injected";
            do
            {
                if (IsQuit(postfix))
                    return null;
                postfix = AskQuestion("Postfix to original directory", def);
            }
            while (!CheckStringAnswer(ref postfix, def, "Postfix cannot be empty."));
            return postfix;
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

        private string AskVersionAssemblyName()
        {
            string name = null;
            do
            {
                if (IsQuit(name))
                    return name;
                name = AskQuestion("Set the assembly name (dll) with extension which contains the actual Product's version", null, false);
            }
            while (!CheckFileNameAnswer(ref name, null, "The assembly name cannot be empty", false));
            return name;
        }

        internal string AskTargetVersonFromUser()
        {
            while (true)
            {
                string version = AskQuestion("Target' version (SemVer format)", null, false);
                //version = "0.8.240"; // "0.8.240-main+2befca57f1"
                var pattern = new Regex(@"\d+(\.\d+)+([-](\p{L})+)?([+]([0-9a-zA-Z])+)?");
                var isMatch = pattern.IsMatch(version);
                if (isMatch)
                    return version;
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
        #endregion
        #region CI
        internal bool CIConfigure()
        {
            _outputHelper.WriteLine("\nSorry, CI operations don't implemented yet", AppConstants.COLOR_TEXT);
            return false;
        }
        #endregion
        #region Common
        private string AskQuestion(string question, string defValue, bool showDefVal = true)
        {
            if (showDefVal)
                question = $"{question} [{defValue}]";
            _outputHelper.WriteLine($"\n{question}: ", AppConstants.COLOR_QUESTION);
            var answer = Console.ReadLine()?.Trim() ?? defValue;
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return answer == "" ? defValue : answer;
        }

        private string AskDirectory(string question, string defValue, bool mustExists, bool showDefVal = true)
        {
            string destDir = null;
            do
            {
                if (IsQuit(destDir))
                    return destDir;
                destDir = AskQuestion(question, defValue, showDefVal);
            }
            while (!CheckDirectoryAnswer(ref destDir, defValue, mustExists));
            return destDir;
        }

        private string AskFilePath(string question, string defValue, bool mustExists, bool showDefVal = true)
        {
            string filePath = null;
            while(true)
            {
                if (IsQuit(filePath))
                    return filePath;
                filePath = AskQuestion(question, defValue, showDefVal);
                string answer = filePath;
                if (!CheckStringAnswer(ref answer, null, "File path cannot be empty", true))
                    continue;
                var dir = Path.GetDirectoryName(filePath);
                if (!CheckDirectoryAnswer(ref dir, defValue, mustExists))
                    continue;
                var fileName = Path.GetFileName(filePath);
                if (!CheckFileNameAnswer(ref fileName, defValue, null, !mustExists))
                    continue;
                break;
            }
            return filePath;
        }

        /// <summary>
        /// Add file log options to the config.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="programName">Name of program</param>
        /// <returns>If false, it is the need to exit from this setup.</returns>
        private bool AddLogFile(InjectorOptions cfg, string programName = "program")
        {
            var answer = AskQuestion($"The {programName} logs will be output to the its console and to a file in the its {LoggerHelper.LOG_FOLDER} folder. Add an additional parallel log file?", "n");
            if (IsYes(answer))
            {
                var logPath = AskFilePath("File path", null, false, false);
                if (IsQuit(logPath))
                    return false;
                //
                var logLevel = LogLevel.Debug;
                while (true)
                {
                    //log level
                    var logTypeS = AskQuestion("Set the log level", logLevel.ToString());
                    if (IsQuit(logPath))
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
                if (cfg.Logs == null)
                    cfg.Logs = new();
                var logData = new LogData()
                {
                    Disabled = false,
                    Type = LogSinkType.File,
                    Path = logPath,
                    Level = logLevel,
                };
                cfg.Logs.Add(logData);
            }
            return true;
        }

        private bool CheckStringAnswer(ref string answer, string defValue, string mess, bool canBeEmpty = false)
        {
            if (!PrimaryCheckInput(ref answer, defValue, out bool noInput))
                return false;
            if (canBeEmpty || !string.IsNullOrWhiteSpace(answer))
            {
                if (noInput)
                    _outputHelper.Write(answer, true, AppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckIntegerAnswer(string answer, string defValue, string mess, int min, int max, out int val)
        {
            val = 0;
            if(!PrimaryCheckInput(ref answer, defValue, out bool noInput))
                return false;
            if (int.TryParse(answer, out val) && val >= min && val <= max)
            {
                if (noInput)
                    _outputHelper.Write(answer, true, AppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckDirectoryAnswer(ref string directory, string defValue, bool mustExist = true)
        {
            if (!PrimaryCheckInput(ref directory, defValue, out bool noInput))
                return false;
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
                //
                if (noInput)
                    _outputHelper.Write(directory, true, AppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine("Directory does not exists.", AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckFileNameAnswer(ref string filename, string defValue, string mess, bool canBeEmpty)
        {
            if (!PrimaryCheckInput(ref filename, defValue, out bool noInput))
                return false;
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
            if (noInput)
                _outputHelper.Write(filename, true, AppConstants.COLOR_ANSWER);
            return true;

        }

        private bool PrimaryCheckInput(ref string answer, string defValue, out bool noInput)
        {
            noInput = answer?.Length == 0; //""
            if (noInput)
                answer = defValue;
            return !IsQuit(answer);
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

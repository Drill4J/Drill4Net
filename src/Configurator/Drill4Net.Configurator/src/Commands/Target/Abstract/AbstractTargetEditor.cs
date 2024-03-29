﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.TypeFinding;
using Drill4Net.Agent.Abstract;

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
            const string appName = CoreConstants.SUBSYSTEM_INJECTOR;
            var injectorDir = _rep.GetInjectorDirectory();
            bool answerBool;

            #region Config
            if (!File.Exists(cfgPath))
            {
                RaiseError($"{appName} config not found: [{cfgPath}]");
                return false;
            }
            var cfg = _rep.ReadInjectionOptions(cfgPath);
            #endregion
            #region Source dir
            string sourceDir = "";
            string def = isNew ? "" : cfg.Source.Directory;
            while(true)
            {
                if (!_cli.AskDirectory($"Target's directory (compiled assemblies). It can be full or relative (for the {appName} program)",
                    out sourceDir, def, true, !string.IsNullOrWhiteSpace(def)))
                    return false;
                break;
            }
            cfg.Source.Directory = sourceDir;
            #endregion
            #region Name
            string trgName = "";
            def = isNew ? GetDefaultTargetName(sourceDir) : cfg.Target.Name;
            do
            {
                if (!_cli.AskQuestion("Target's name (as Agent ID). It must be the same for the different builds of the Product", out trgName, def))
                    return false;
            }
            while (!_cli.CheckStringAnswer(ref trgName, "Target's name cannot be empty", false));
            var corrTrgName = CorrectTargetName(trgName);
            if (!corrTrgName.Equals(trgName, StringComparison.InvariantCultureIgnoreCase))
            {
                RaiseWarning(corrTrgName);
            }
            cfg.Target.Name = corrTrgName;
            #endregion
            #region Description
            def = isNew ? "" : cfg.Description;
            if (!_cli.AskQuestion("Target's description", out string desc, def, !string.IsNullOrWhiteSpace(def)))
                return false;
            cfg.Description = desc;
            #endregion
            #region Version
            if (cfg.Target == null)
                cfg.Target = new();
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

The filters:
  - By directory (full path). Examples:
       {ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\  (include rule by default)
       {ConfiguratorConstants.FILTER_TYPE_DIR}=^d:\Projects\ABC\  (exclude rule - note the ^ sign at the beginning of the value)
       {ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\,d:\Projects\123\  (two directories in one type's tag with comma)
       {ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\ABC\;{ConfiguratorConstants.FILTER_TYPE_DIR}=d:\Projects\123\ (two directories in separate blocks with semicolon)
  - By folder (just name). Example: 
       {ConfiguratorConstants.FILTER_TYPE_FOLDER}=backup1
  - By file (short name). Example: 
       {ConfiguratorConstants.FILTER_TYPE_FILE}=Drill4Net.Target.Common.dll
  - By namespace (by beginning if value is presented as string, or by regex). Examples:
       {ConfiguratorConstants.FILTER_TYPE_NAMESPACE}=Drill4Net
       {ConfiguratorConstants.FILTER_TYPE_NAMESPACE}=reg: ^Drill4Net\.([\w-]+\.)+[\w]*Tests$
  - By class fullname (with namespace). Example:
       {ConfiguratorConstants.FILTER_TYPE_CLASS}=Drill4Net.Target.Common.ModelTarget
  - By attribute of type. Example:
       {ConfiguratorConstants.FILTER_TYPE_ATTRIBUTE}=Drill4Net.Target.SuperAttribute  (this is full name of its type)

Hint: the regex must be located only in a sole filter tag (one expression in one tag).
Hint: to set up value for ""all entities of current filter type"" use sign *. Example:
       {ConfiguratorConstants.FILTER_TYPE_FOLDER}=^*  (do not process any folders)

To finish, just enter ""{ConfiguratorConstants.ANSWER_OK}"".";
            RaiseMessage(filterHint, CliMessageType.Help);

            const string? filterQuestion = "Please create at least one filter rule (or skip this step now, but in this case you will need to manually create the above rules directly in the saved config):";
            RaiseQuestion($"\n{filterQuestion}");

            while (true)
            {
                answer = Console.ReadLine()?.Trim() ?? "";
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
            var cfgHelper = new InjectionOptionsHelper();
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
                        if (!_cli.AskDirectory("Destination's directory (processed assemblies). It may not exist yet. BE CAREFUL: the targets output folder is intended exclusively for this instrumented target. This is exactly the new catalog. Do not use the root of system folders, user folders, your data folders! During the injection process, it will be cleaned!",
                            out var destDir, null, false, false))
                            return false;
                        cfg.Destination.Directory = destDir;
                        break;
                    default:
                        continue;
                }
                //
                try
                {
                    cfgHelper.SetDestinationDirectory(cfg, null);
                    cfgHelper.ValidateOptions(cfg, false);
                } 
                catch (Exception ex)
                {
                    RaiseWarning(ex.Message);
                    continue;
                }
                //
                break;
            }
            #endregion
            #region Plugins
            if (!_cli.AskQuestionBoolean("Does the target have any automated tests?", out answerBool, true))
                return false;
            if (answerBool)
            {
                RaiseMessage("\nNow you need specify only necessary agent's \"Generator contexter plugins\" which intercept the tests' execution workflow and retrieve their context (implemented IGeneratorContexter interface - for example, for SpecFlow framework. For simple test ones, such as xUnit, NUnit, or MsTest you don't need to do it here).");
                if (cfg.Plugins == null)
                    cfg.Plugins = new();
                var plugins = cfg.Plugins;
                if (isNew)
                {
                    plugins.Clear(); //clear plugins from the model config

                    #region Search the plugins
                    var dir = _rep.GetAgentPluginDirectory();
                    List<Type> plugTypes = new();
                    try
                    {
                        RaiseMessage($"Search for existing plugins in the [{dir}]. Please, wait...");
                        plugTypes = GetGeneratorPlugins(dir).OrderBy(a => a.Name).ToList();
                    }
                    catch (Exception ex)
                    {
                        var er = ex.ToString();
                        _logger.Error(er);
                        RaiseWarning($"Error: {er}");

                    }
                    #endregion
                    #region Create the plugin section
                    if (plugTypes.Count == 0)
                    {
                        RaiseWarning("No plugins detected.");
                    }
                    else
                    {
                        RaiseMessage("\nThe found plugins are:");
                        for (int i = 0; i < plugTypes.Count; i++)
                        {
                            var type = plugTypes[i];
                            RaiseMessage($"{i + 1}. {type.Name}", CliMessageType.Info);
                        }
                        RaiseMessage("\nType the plugin number. When you're done, type ok.");
                        //
                        Type plugType;
                        while (true)
                        {
                            try
                            {
                                #region Select plugin
                                if (!_cli.AskQuestion("Plugin number", out answer, null, false))
                                    return false;
                                if (_cli.IsOk(answer))
                                    break;
                                if (string.IsNullOrWhiteSpace(answer))
                                {
                                    RaiseWarning("Input cannot be empty");
                                    continue;
                                }
                                answer = answer.Trim();
                                if (!int.TryParse(answer, out var num) || num < 1 || num > plugTypes.Count)
                                {
                                    RaiseWarning("Out of range, please repeat");
                                    continue;
                                }
                                //
                                plugType = plugTypes[num - 1];
                                var plugDir = Path.GetDirectoryName(plugType.Assembly.Location);
                                var plug = Activator.CreateInstance(plugType) as IGeneratorContexter ?? throw new NullReferenceException($"The plugin type is wrong: [{plugType.FullName}]");
                                var plugName = plug.Name;
                                #endregion
                                #region Plugin config path
                                def = $"plug_{trgName}.yml";
                                string plugCfgPath = "";
                                while (true)
                                {
                                    if (!_cli.AskFileNameOrPath(@$"Name or full path file for the 'target specific config' for the plugin ""{plugName}"". Please create this configuration outside of the configurator manually - read the documentation for a specific plugin. Usually the model configuration can be located in the plugin directory, which you can use as a template. The best practice is to get the ""plug_"" prefix for the concrete target specific files.",
                                        out var plugCfgName, def, false))
                                        return false;
                                    if (string.IsNullOrWhiteSpace(plugCfgName))
                                    {
                                        RaiseWarning("The target specific config name is empty");
                                        continue;
                                    }
                                    if (!plugCfgName.EndsWith(".yml"))
                                        plugCfgName += ".yml";

                                    if (plugCfgName.Contains(Path.DirectorySeparatorChar))
                                        plugCfgPath = plugCfgName;
                                    else
                                        plugCfgPath = Path.Combine(injectorDir, plugCfgName);

                                    if (File.Exists(plugCfgPath))
                                        break;
                                    RaiseWarning(@$"The target specific config for the ""{trgName}"" and the plugin ""{plugName}"" does not exist: [{plugCfgPath}]");
                                }
                                #endregion

                                if (!string.IsNullOrWhiteSpace(plugCfgPath))
                                {
                                    var plugOpts = new PluginLoaderOptions()
                                    {
                                        Directory = plugDir,
                                        Config = plugCfgPath,
                                    };
                                    plugins.Add(plugName, plugOpts);
                                }
                            }
                            catch (Exception ex)
                            {
                                RaiseError(ex.Message);
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    RaiseWarning($"The program does not yet implement editing information about the plugin in the configuration. Please do it manually in [{cfgPath}]");
                }
            }
            #endregion
            #region Logs
            if (cfg.Logs == null)
                cfg.Logs = new();
            if (!_cli.AddLogFile(cfg.Logs, appName))
                return false;
            #endregion

            //corrections
            cfg.Profiler.Directory = _rep.GetTransmitterDir();
            ClarifyProfilerDirectory(cfg.Profiler);

            ClarifyPluginDirectories(cfg.Plugins);

            // save config
            if (isNew)
            {
                RaiseMessage($"\n{HelpHelper.GetInjectorAndRunnerConfigSavingNote(CoreConstants.SUBSYSTEM_INJECTOR)}");
                return _cmdHelper.AskNameAndSave(appName, cfg, injectorDir, true);
            }
            else
            {
                return _cmdHelper.SaveConfig(appName, cfg, cfgPath);
            }
        }

        /// <summary>
        /// Get the IGeneratorContexter plugins (SpecFlow, etc) for Transmitter
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal List<Type> GetGeneratorPlugins(string dir)
        {
            //search the plugins
            var pluginator = new TypeFinder();
            var filter = new SourceFilterOptions
            {
                Excludes = new SourceFilterParams
                {
                    Files = new List<string>
                    {
                        "reg:.resources.dll$",
                    },
                },
            };
            List<Type> ctxDirs = new();
            try
            {
                ctxDirs = pluginator.GetBy(TypeFinderMode.Interface, dir, nameof(IGeneratorContexter), filter);
            }
            catch (Exception ex)
            {
                var er = $"Search for Transmitter plugins (IGeneratorContexter) is failed in [{dir}]";
                _logger.Fatal(er, ex);
                RaiseWarning(er);
            }
            return ctxDirs;
        }

        internal void ClarifyProfilerDirectory(ProfilerOptions opts)
        {
            var dir = opts.Directory;
            if (Path.IsPathRooted(dir)) //absolute path
                return;
            opts.Directory = FileUtils.GetFullPath(dir, _rep.GetInjectorDirectory());
        }

        internal void ClarifyPluginDirectories(Dictionary<string, PluginLoaderOptions> plugs)
        {
            var baseDir = _rep.GetInjectorDirectory();
            foreach (var plug in plugs.Values)
            {
                var dir = plug.Directory;
                if (Path.IsPathRooted(dir)) //absolute path
                    continue;
                plug.Directory = FileUtils.GetFullPath(dir, baseDir);
            }
        }

        private bool AskDestinationPostfix(ref string postfix)
        {
            var def = string.IsNullOrWhiteSpace(postfix) ? CoreConstants.INJECTION_DESTITANTION_POSTFIX : postfix;
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
            return CorrectTargetName(new DirectoryInfo(sourceDir).Name);
        }

        internal string CorrectTargetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "target-default";
            name = ReplaceSpecialSymbolsForTargetName(name);

            //split name by "-" for Camel notation
            var sb = new StringBuilder();
            var prevHigh = false;
            for (int i = 0; i < name.Length; i++)
            {
                var ch = name[i];
                var isUpper = char.IsUpper(ch);
                if (isUpper && !prevHigh && i > 0 && name[i - 1] != '-')
                    sb.Append('-');
                sb.Append(ch);
                prevHigh = isUpper;
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

                //get rules' values
                var val = ar[1];
                List<string> vals;
                if (val.Contains(','))
                    vals = val.Split(",").ToList();
                else
                    vals = new() { val };

                // add the rules' values
                var type = ar[0].Trim().ToUpper();
                foreach (var curVal in vals)
                {
                    var isExclude = curVal.StartsWith("^");
                    SourceFilterParams pars = isExclude ? filter.Excludes : filter.Includes;
                    var actualVal = (isExclude ? curVal[1..] : curVal)?.Trim();
                    switch (type)
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

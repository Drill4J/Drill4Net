using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;

namespace Drill4Net.Configurator
{
    public class CommandHelper : CliMessager
    {
        public CliInteractor Cli { get; }

        private readonly ConfiguratorRepository _rep;
        protected readonly Logger _logger;

        /*****************************************************************/

        public CommandHelper(CliInteractor cli, ConfiguratorRepository rep): base(cli.Id)
        {
            Cli = cli ?? throw new ArgumentNullException(nameof(cli));
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<AbstractCliCommand>(rep.Subsystem);
        }

        /*****************************************************************/

        internal bool AskNameAndSave<T>(string cfgSubsystem, T cfg, string dir, bool activate = false) where T : AbstractOptions, new()
        {
            string cfgPath;
            var needSave = true;

            //questions
            while (true)
            {
                if (!Cli.AskQuestion($"Name of the {cfgSubsystem} config", out var name, CoreConstants.CONFIG_NAME_DEFAULT))
                    return false;
                if (!Cli.CheckFileNameAnswer(ref name, "Wrong file name", false))
                    continue;
                if (!Path.HasExtension(name))
                    name += ".yml";
                cfgPath = Path.Combine(dir, name);

                if (File.Exists(cfgPath))
                {
                    if (!Cli.AskQuestion("Such name already exists. Replace?", out var answer, "n"))
                        return false;
                    needSave = Cli.IsYes(answer);
                }
                break;
            }

            //saving
            if (needSave)
            {
                if (!SaveConfig(cfgSubsystem, cfg, cfgPath))
                    return false;

                //activating
                if (activate)
                {
                    (var needActivate, var redirectCfgPath) = IsNeedAcivateConfigFor(dir, cfgPath);
                    if (needActivate)
                        return SaveRedirectFile(cfgSubsystem, cfgPath, redirectCfgPath);
                }
            }
            return true;
        }

        internal bool SaveConfig<T>(string cfgSubsystem, T cfg, string cfgPath) where T : AbstractOptions, new()
        {
            try
            {
                _rep.WriteOptions<T>(cfg, cfgPath);
            }
            catch (Exception ex)
            {
                var er = $"Config for {cfgSubsystem} is not saved:\n{ex}";
                _logger.Error(er);
                RaiseError(er);
                return false;
            }
            _logger.Info($"Config for {cfgSubsystem} saved to [{cfgPath}]");
            RaiseMessage($"Config is saved. You can check the {cfgSubsystem}'s settings: {cfgPath}", CliMessageType.Info);
            return true;
        }

        internal bool DeleteConfig<T>(string cfgSubsystem, string dir, CliDescriptor desc, out string sourcePath)
            where T : AbstractOptions, new()
        {
            // source path
            var res = GetSourceConfigPath<T>(cfgSubsystem, dir, desc, out sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return false;
            }

            // ask
            var forceDelete = desc.IsSwitchSet(CoreConstants.SWITCH_FORCE);
            if (!forceDelete)
            {
                //to delete the actual config in redirecting file is bad idea
                var actualCfg = _rep.GetActualConfigPath(dir);
                string answer;
                if (actualCfg.Equals(sourcePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!Cli.AskQuestion($"The {cfgSubsystem} config [{sourcePath}] is active in the redirecting file.\nDo you want to delete it? Answer",
                        out answer, "n"))
                        return false;
                    if (!Cli.IsYes(answer))
                        return false;
                }
                //
                if (!Cli.AskQuestion($"Delete the {cfgSubsystem} config [{sourcePath}]?", out answer, "y"))
                    return false;
                if (!Cli.IsYes(answer))
                    return false;
            }

            //output
            try
            {
                File.Delete(sourcePath);
            }
            catch (Exception ex)
            {
                var er = ex.ToString();
                _logger.Error(er);
                RaiseError(er);
                return false;
            }
            RaiseMessage($"{cfgSubsystem} config was deleted: [{sourcePath}]", CliMessageType.Info);

            return true;
        }

        internal bool ActivateConfig<T>(string cfgSubsystem, string dir, CliDescriptor desc) where T : AbstractOptions, new()
        {
            //source path
            var res = GetSourceConfigPath<T>(cfgSubsystem, dir, desc, out var sourcePath,
                out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return false;
            }

            //activate
            var path = _rep.CalcRedirectConfigPath(dir);
            SaveRedirectFile(cfgSubsystem, Path.GetFileNameWithoutExtension(sourcePath), //better set just file name but its path 
                path);

            return true;
        }

        internal (bool, string) IsNeedAcivateConfigFor(string appDir, string curCfgPath)
        {
            var redirectCfgPath = _rep.CalcRedirectConfigPath(appDir);
            var name = Path.GetFileName(curCfgPath);
            var isDefName = name.Equals(CoreConstants.CONFIG_NAME_DEFAULT, StringComparison.InvariantCultureIgnoreCase);
            bool needActivate;
            if (File.Exists(redirectCfgPath))
            {
                var redirData = _rep.ReadRedirectData(redirectCfgPath);
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

        internal bool ViewFile<T>(string cfgSubsystem, string dir, CliDescriptor desc, out string sourcePath) where T : AbstractOptions, new()
        {
            // sorce path
            var res = GetSourceConfigPath<T>(cfgSubsystem, dir, desc, out sourcePath,
                out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return false;
            }

            //output
            var text = File.ReadAllText(sourcePath);
            RaiseMessage(text);
            return true;
        }

        internal bool SaveRedirectFile(string cfgSubsystem, string actualPath, string redirectCfgPath)
        {
            try
            {
                _rep.WriteRedirectData(new RedirectData { Path = actualPath }, redirectCfgPath);
                _logger.Info($"Redirect config for {cfgSubsystem} saved to [{redirectCfgPath}]");
                RaiseMessage($"The {cfgSubsystem} config [{actualPath}] is active now");
                return true;
            }
            catch (Exception ex)
            {
                var er = $"Redirect config for {cfgSubsystem} is not saved:\n{ex}";
                _logger.Error(er);
                RaiseError(er);
                return false;
            }
        }

        internal bool GetSourceConfigPath<T>(string cfgSubsystem, string dir, CliDescriptor desc,
            out string path, out bool fromSwitch, out string error) where T : AbstractOptions, new()
        {
            path = string.Empty;
            error = string.Empty;
            fromSwitch = false;

            var sourceName = desc.GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH);
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                //switches
                var copyActive = desc.IsSwitchSet(ConfiguratorConstants.SWITCH_ACTIVE); //copy active
                if (copyActive)
                {
                    var actualCfg = _rep.GetActualConfigPath(dir);
                    sourceName = Path.GetFileName(actualCfg);
                    dir = Path.GetDirectoryName(actualCfg);
                }
                if (string.IsNullOrWhiteSpace(sourceName))
                {
                    var copyLast = desc.IsSwitchSet(ConfiguratorConstants.SWITCH_LAST);
                    if (copyLast)
                    {
                        var configs = _rep.GetConfigs<T>(cfgSubsystem, dir);
                        var lastEditedFile = string.Empty;
                        var dt = DateTime.MinValue;
                        foreach (var config in configs)
                        {
                            var fdt = File.GetLastWriteTime(config);
                            if (fdt < dt)
                                continue;
                            dt = fdt;
                            lastEditedFile = config;
                        }
                        if (lastEditedFile != string.Empty)
                            sourceName = Path.GetFileName(lastEditedFile);
                    }
                }

                fromSwitch = !string.IsNullOrWhiteSpace(sourceName);
            }

            //check file name
            if(string.IsNullOrWhiteSpace(sourceName))
                sourceName = desc.GetPositional(0);
            if (!string.IsNullOrWhiteSpace(sourceName))
            {
                //in fact, it is full path
                if (!string.IsNullOrWhiteSpace(Path.GetPathRoot(sourceName)))
                {
                    sourceName = sourceName.Replace("\"", null);
                    dir = Path.GetDirectoryName(sourceName);
                    sourceName = Path.GetFileName(sourceName);
                }
            }

            //source path
            return GetConfigPath(dir, "source", sourceName, true, out path, out error);
        }

        internal bool GetConfigPath(string dir, string typeConfig, string name, bool mustExist, out string path, out string error)
        {
            path = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                error = $"The {typeConfig} config is not specified, see help.";
                return false;
            }
            name = name.Replace("\"", null);
            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(name)))
            {
                error = $"The {typeConfig} config should be just a file name without a directory, see help.";
                return false;
            }
            if (!name.EndsWith(".yml"))
                name += ".yml";

            path = Path.Combine(dir, name);
            if (mustExist && !File.Exists(path))
            {
                error = $"The {typeConfig} config not found: [{path}]";
                return false;
            }

            return true;
        }

        internal void ListConfigs<T>(string cfgSubsystem, string dir) where T : AbstractOptions, new()
        {
            var configs = _rep.GetConfigs<T>(cfgSubsystem, dir)
                .OrderBy(a => a).ToArray();
            var actualPath = _rep.GetActualConfigPath(dir);
            for (int i = 0; i < configs.Length; i++)
            {
                string? path = configs[i];
                var isActual = path.Equals(actualPath, StringComparison.InvariantCultureIgnoreCase);
                var a = isActual ? " <<" : "";
                var name = Path.GetFileNameWithoutExtension(path);
                RaiseMessage($"{i + 1}. {name}{a}", CliMessageType.Info);
            }
        }

        internal bool OpenConfig<T>(string cfgSubsystem, string dir, CliDescriptor desc) where T : AbstractOptions, new()
        {
            var res = GetSourceConfigPath<T>(cfgSubsystem, dir, desc, out var fileName, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return false;
            }
            return OpenFile(fileName);
        }

        internal bool OpenFile(string fileName)
        {
            var edPath = _rep.GetExternalEditor();
            if (string.IsNullOrWhiteSpace(edPath))
            {
                System.Diagnostics.Process.Start(fileName);
            }
            if (!File.Exists(edPath))
            {
                RaiseWarning("The specified external editor does not found");
                return false;
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                RaiseWarning("The file for editing is empty");
                return false;
            }
            fileName = fileName.Replace("\"", null);
            if (!File.Exists(fileName))
            {
                RaiseWarning("The specified file for editing does not found");
                return false;
            }
            System.Diagnostics.Process.Start(edPath, fileName);

            return true;
        }

        internal void ViewLogOptions(List<LogData> logs)
        {
            if (logs == null || logs.Count == 0)
                return;
            RaiseMessage("Additional logs:");
            foreach (LogData logData in logs)
                RaiseMessage($"  -- {logData}");
        }

        internal bool CheckVersions(string check, string sourceDir, VersionData? versData)
        {
            var globRes = true;

            // section can be empty
            if (versData == null)
                return true;
            if (string.IsNullOrWhiteSpace(sourceDir))
            {
                RegCheck(check, "Can't check the version section due to source directory path is empty", false, ref globRes);
                return false;
            }
            //
            var res = true;
            //var dir = versData.Directory; //used only for tests' system
            foreach (var moniker in versData.Targets.Keys)
            {
                var monData = versData.Targets[moniker];

                // base dir
                var rootDir = Path.Combine(monData.BaseFolder, sourceDir);
                var res2 = Directory.Exists(rootDir);
                if (!res2)
                {
                    RegCheck(check, $"Directory for {moniker} does not exist: [{rootDir}]", false, ref globRes);
                    res = false;
                }

                // folders (used fot Test Engine, not for common targets)
                if (monData.Folders != null)
                {
                    foreach (var fldData in monData.Folders)
                    {
                        var asmFld = fldData.Folder; //can be empty
                        if (string.IsNullOrWhiteSpace(asmFld))
                            asmFld = rootDir;
                        //
                        var asmDir = FileUtils.GetFullPath(asmFld, rootDir);
                        res2 = Directory.Exists(asmDir);
                        if (!res2)
                        {
                            RegCheck(check, $"Directory for {moniker} and folder {asmFld} does not exist: [{asmDir}]", false, ref globRes);
                            res = false;
                        }

                        //assemblies
                        if (fldData.Assemblies != null)
                        {
                            foreach (var asmName in fldData.Assemblies.Keys)
                            {
                                var asmPath = Path.Combine(asmDir, asmName);
                                res2 = File.Exists(asmPath);
                                if (!res2)
                                {
                                    RegCheck(check, $"Assembly for {moniker} and folder {asmFld} not found: [{asmPath}]", false, ref globRes);
                                    res = false;
                                }
                                //
                                //var types = fldData.Assemblies[asmName]; //We are not checking this yet
                            }
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Registering the result of current operation of the <see cref="AbstractCliCommand"/> object
        /// </summary>
        /// <param name="check"></param>
        /// <param name="error"></param>
        /// <param name="res"></param>
        /// <param name="cmdRes">The result for current command</param>
        internal void RegCheck(string check, string error, bool res, ref bool cmdRes)
        {
            if (res)
            {
                RaiseMessage($"{check}: OK", CliMessageType.Info);
            }
            else
            {
                RaiseError($"{check}: NOT");
                //
                error = error.Trim();
                if (!string.IsNullOrWhiteSpace(error))
                    error = char.ToLower(error[0]) + error[1..];
                RaiseError($"Reason: {error}");
            }
            //
            if (!res)
                cmdRes = false;
        }

        /// <summary>
        /// Registering the result of some check.
        /// </summary>
        /// <param name="cmdRes">The result of current <see cref="AbstractCliCommand"/> object</param>
        /// <param name="isFinalRes">The final group result when several command are run in a group</param>
        internal void RegResult(bool cmdRes, bool isFinalRes = false)
        {
            var res = cmdRes ? "OK" : "NOT";
            var final = isFinalRes ? "FINAL " : "";
            RaiseMessage($"\n{final}RESULT: {res}", cmdRes ? CliMessageType.Info : CliMessageType.Error);
        }

        internal void SetCommandCheckResult((bool done, Dictionary<string, object> results) res, ref bool cmdRes)
        {
            if (!res.done)
            {
                cmdRes = false;
            }
            else
            {
                var results = res.results;
                if (results == null || !results.ContainsKey(ConfiguratorConstants.CHECK_KEY))
                {
                    cmdRes = false;
                }
                else
                {
                    if (results[ConfiguratorConstants.CHECK_KEY]?.ToString()?
                        .Equals(ConfiguratorConstants.CHECK_VALUE_NOT, StringComparison.InvariantCultureIgnoreCase) == true)
                        cmdRes = false;
                }
            }
        }
    }
}

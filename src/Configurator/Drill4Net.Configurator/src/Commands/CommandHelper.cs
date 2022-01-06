using System;
using System.IO;
using System.Linq;
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

        internal bool AskNameAndSave<T>(string appName, T cfg, string dir, bool activate = false) where T : AbstractOptions, new()
        {
            string cfgPath;
            var needSave = true;

            //questions
            while (true)
            {
                if (!Cli.AskQuestion($"Name of the {appName}'s config", out var name, CoreConstants.CONFIG_NAME_DEFAULT))
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
                if (!SaveConfig(appName, cfg, cfgPath))
                    return false;

                //activating
                if (activate)
                {
                    (var needActivate, var redirectCfgPath) = IsNeedAcivateConfigFor(dir, cfgPath);
                    if (needActivate)
                        return SaveRedirectFile(appName, cfgPath, redirectCfgPath);
                }
            }
            return true;
        }

        internal bool SaveConfig<T>(string appName, T cfg, string cfgPath) where T : AbstractOptions, new()
        {
            try
            {
                _rep.WriteOptions<T>(cfg, cfgPath);
            }
            catch (Exception ex)
            {
                var er = $"Config for {appName} is not saved:\n{ex}";
                _logger.Error(er);
                RaiseError(er);
                return false;
            }
            _logger.Info($"Config for {appName} saved to [{cfgPath}]");
            RaiseMessage($"Config is saved. You can check the {appName}'s settings: {cfgPath}", CliMessageType.Info);
            return true;
        }

        internal bool DeleteConfig<T>(string appName, string dir, AbstractCliCommand cmd) where T : AbstractOptions, new()
        {
            // source path
            var res = GetSourceConfig<T>(appName, dir, cmd, out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return false;
            }

            // ask
            var forceDelete = cmd.IsSwitchSet('f');
            if (!forceDelete)
            {
                //to delete the actual config in redirecting file is bad idea
                var actualCfg = _rep.GetActualConfigPath(dir);
                string answer;
                if (actualCfg.Equals(sourcePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!Cli.AskQuestion($"The {appName}'s config [{sourcePath}] is active in the redirecting file.\nDo you want to delete it? Answer", out answer, "n"))
                        return false;
                    if (!Cli.IsYes(answer))
                        return false;
                }
                //
                if (!Cli.AskQuestion($"Delete the {appName}'s config [{sourcePath}]?", out answer, "y"))
                    return false;
                if (!Cli.IsYes(answer))
                    return false;
            }

            //output
            File.Delete(sourcePath);
            RaiseMessage($"{appName}'s config is deleted: [{sourcePath}]", CliMessageType.Info);

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

        internal bool SaveRedirectFile(string appName, string actualPath, string redirectCfgPath)
        {
            try
            {
                _rep.WriteRedirectData(new RedirectData { Path = actualPath }, redirectCfgPath);
                _logger.Info($"Redirect config for {appName} saved to [{redirectCfgPath}]");
                RaiseMessage($"The {appName}'s config [{actualPath}] is active now");
                return true;
            }
            catch (Exception ex)
            {
                var er = $"Redirect config for {appName} is not saved:\n{ex}";
                _logger.Error(er);
                RaiseError(er);
                return false;
            }
        }

        internal bool GetSourceConfig<T>(string subsystem, string dir, AbstractCliCommand cmd,
            out string path, out bool fromSwitch, out string error) where T : AbstractOptions, new()
        {
            var sourceName = string.Empty;
            path = string.Empty;
            error = string.Empty;

            //switches
            var copyActive = cmd.IsSwitchSet('a'); //copy active
            if (copyActive)
            {
                var actualCfg = _rep.GetActualConfigPath(dir);
                sourceName = Path.GetFileName(actualCfg);
            }
            if (sourceName == string.Empty)
            {
                var copyLast = cmd.IsSwitchSet('l');
                if (copyLast)
                {
                    var configs = _rep.GetConfigs<T>(subsystem, dir);
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

            if (string.IsNullOrWhiteSpace(sourceName))
                sourceName = cmd.GetPositional(0);

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

        internal void ListConfigs<T>(string subsystem, string dir) where T : AbstractOptions, new()
        {
            var configs = _rep.GetConfigs<T>(subsystem, dir)
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

        internal bool OpenConfig<T>(string subsystem, string dir, AbstractCliCommand cmd) where T : AbstractOptions, new()
        {
            var res = GetSourceConfig<T>(subsystem, dir, cmd, out var fileName, out var _, out var error);
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
    }
}

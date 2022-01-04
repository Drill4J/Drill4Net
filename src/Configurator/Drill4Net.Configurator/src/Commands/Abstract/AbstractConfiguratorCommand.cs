using System;
using System.IO;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Configurator
{
    public abstract class AbstractConfiguratorCommand : AbstractCliCommand
    {
        protected readonly CommandHelper _commandHelper;
        protected readonly ConfiguratorRepository _rep;

        /****************************************************************************/

        protected AbstractConfiguratorCommand(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _commandHelper = new(rep);
        }

        /****************************************************************************/

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
    }
}

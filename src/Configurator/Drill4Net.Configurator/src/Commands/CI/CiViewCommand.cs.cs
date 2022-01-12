using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_VIEW)]
    public class CiViewCommand : AbstractConfiguratorCommand
    {
        public CiViewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep): base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            //
            string cfgPath;
            var noContent = IsSwitchSet(ConfiguratorConstants.SWITCH_CONTENT_NO);
            var cfgDir = _rep.GetCiDirectory();
            
            // view content of the config
            bool res;
            if (!noContent)
            {
                res = _cmdHelper.ViewFile<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath);
            }
            else
            {
                res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath,
                    out _, out var error);
                if (!res)
                    RaiseError(error);
            }
            if (!res)
                return Task.FromResult(FalseEmptyResult);
            
            // view projects with the config's integration
            var viewIntegration = IsSwitchSet(ConfiguratorConstants.SWITCH_INTEGRATION);
            var solutionDir = GetParameter(CoreConstants.ARGUMENT_SOURCE_DIR, false);
            if (viewIntegration || solutionDir != null)
            {
                var ide = new IdeConfigurator(_rep);
                if (solutionDir == null)
                {
                    var def = ide.GetDefaultProjectSourcesDirectory();
                    var needSpecify = IsSwitchSet(ConfiguratorConstants.SWITCH_DEFAULT_NO);
                    if (needSpecify)
                    {
                        if (!_cli.AskDirectory(ConfiguratorConstants.MESSAGE_CI_INTEGRATION_IDE_DIR,
                            out solutionDir, def, true))
                            return Task.FromResult(TrueEmptyResult);
                    }
                    else
                    {
                        solutionDir = def;
                    }
                }
                //
                var prjs = ide.GetProjectsWithCiIntegrations(solutionDir, cfgPath);
                if (prjs.Count == 0)
                {
                    RaiseMessage("No integrations were found in the projects.", CliMessageType.Info);
                }
                else
                {
                    RaiseMessage("\nThe projects with IDE's integrations are:", CliMessageType.Info);
                    foreach (var prj in prjs)
                        RaiseMessage(prj, CliMessageType.Info);
                }
            }
            //
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return $"View the content of specified {CoreConstants.SUBSYSTEM_CI}'s config.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

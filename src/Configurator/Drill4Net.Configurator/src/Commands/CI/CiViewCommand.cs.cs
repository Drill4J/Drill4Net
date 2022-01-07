﻿using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_VIEW)]
    public class CiViewCommand : AbstractConfiguratorCommand
    {
        public CiViewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var cfgDir = _rep.GetCiDirectory();
            var res = _cmdHelper.ViewFile<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out var cfgPath);
            if(!res)
                return Task.FromResult(false);
            //
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
                            return Task.FromResult(true);
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
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"View the content of specified {CoreConstants.SUBSYSTEM_CI}'s config";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

﻿using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_VIEW)]
    public class TargetViewCommand : AbstractConfiguratorCommand
    {
        public TargetViewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            // sorce path
            var res = _commandHelper.GetSourceConfig(dir, this, out var sourcePath, out var fromSwitch, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            //output
            var text = File.ReadAllText(sourcePath);
            RaiseMessage(text);

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "View the content of specified Injector's config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
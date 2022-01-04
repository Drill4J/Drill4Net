﻿using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TargetEditCommand : AbstractTargetEditor
    {
        public TargetEditCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            // source path
            var res = _commandHelper.GetSourceConfig(dir, this, out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            res = Edit(sourcePath, false);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return "Edit the specified Injector's config in interactive mode";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
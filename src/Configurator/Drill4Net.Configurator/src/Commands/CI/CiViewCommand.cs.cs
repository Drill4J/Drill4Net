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
            throw new System.NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}

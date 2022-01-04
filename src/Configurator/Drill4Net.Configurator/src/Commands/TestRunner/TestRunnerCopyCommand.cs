﻿using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_COPY)]
    public class TestRunnerCopyCommand : AbstractConfiguratorCommand
    {
        public TestRunnerCopyCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
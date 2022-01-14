﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_INJECT)]
    public class TargetInjectCommand : AbstractConfiguratorCommand
    {
        public TargetInjectCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            RaiseWarning("This command does not implemented yet");
            return Task.FromResult(FalseEmptyResult);
        }

        public override string GetShortDescription()
        {
            return "";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
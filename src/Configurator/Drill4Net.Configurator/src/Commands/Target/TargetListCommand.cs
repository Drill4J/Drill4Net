﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Repository;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_LIST)]
    public class TargetListCommand : AbstractConfiguratorCommand
    {
        public TargetListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /******************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();
            var configs = _rep.GetInjectorConfigs(dir)
                .OrderBy(a => a).ToArray();
            var actualCfg = new BaseOptionsHelper(_rep.Subsystem)
                .GetActualConfigPath(dir);
            for (int i = 0; i < configs.Length; i++)
            {
                string? file = configs[i];
                var isActual = file.Equals(actualCfg, StringComparison.InvariantCultureIgnoreCase);
                var a1 = isActual ? "[" : "";
                var a2 = isActual ? "]" : "";
                var name = Path.GetFileNameWithoutExtension(file);
                RaiseMessage($"{a1}{i+1}{a2}. {name}", CliMessageType.Info);
            }
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Get list of the Injector's configs";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;

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
            var actualPath = _rep.GetActualConfigPath(dir);
            for (int i = 0; i < configs.Length; i++)
            {
                string? path = configs[i];
                var isActual = path.Equals(actualPath, StringComparison.InvariantCultureIgnoreCase);
                var a = isActual ? ">>" : "";
                var name = Path.GetFileNameWithoutExtension(path);
                RaiseMessage($"{i+1}. {a}{name}", CliMessageType.Info);
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

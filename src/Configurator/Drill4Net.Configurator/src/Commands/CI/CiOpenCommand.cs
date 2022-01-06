using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_OPEN)]
    public class CiOpenCommand : AbstractConfiguratorCommand
    {
        public CiOpenCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CONFIGURATOR, dir, this, out var sourcePath,
                out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }
            res = _cmdHelper.OpenFile(sourcePath);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return "Open the config for CI in external editor";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}

using System.Threading.Tasks;
using Drill4Net.Cli;

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
            var path = GetPositional(0);
            var res = _cmdHelper.OpenFile(path);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Open the config for CI in external editor";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}

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
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.OpenConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Open the config for {CoreConstants.SUBSYSTEM_CI}'s in external editor";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

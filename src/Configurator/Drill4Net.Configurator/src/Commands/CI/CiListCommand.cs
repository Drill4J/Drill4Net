using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_LIST)]
    public class CiListCommand : AbstractConfiguratorCommand
    {
        public CiListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetCiDirectory();
            _cmdHelper.ListConfigs<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Get list of the {CoreConstants.SUBSYSTEM_CI}'s configs";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

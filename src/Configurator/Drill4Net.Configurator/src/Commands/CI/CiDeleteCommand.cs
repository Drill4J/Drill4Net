using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_DELETE)]
    public class CiDeleteCommand : AbstractConfiguratorCommand
    {
        public CiDeleteCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.DeleteConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_CI}'s config";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}

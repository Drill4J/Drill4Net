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
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.DeleteConfig<CiOptions>(CoreConstants.SUBSYSTEM_CONFIGURATOR, dir, this);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return "Delete the specified CI config";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}

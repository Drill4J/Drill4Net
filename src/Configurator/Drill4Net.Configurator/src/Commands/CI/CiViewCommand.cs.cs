using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_VIEW)]
    public class CiViewCommand : AbstractConfiguratorCommand
    {
        public CiViewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetCiDirectory();
            return Task.FromResult(_cmdHelper.ViewFile<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir, _desc));
        }

        public override string GetShortDescription()
        {
            return $"View the content of specified {CoreConstants.SUBSYSTEM_CI}'s config";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

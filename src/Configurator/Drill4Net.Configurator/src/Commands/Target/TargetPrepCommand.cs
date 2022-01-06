using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_PREP)]
    public class TargetPrepCommand : AbstractConfiguratorCommand
    {
        public TargetPrepCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            throw new System.NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}

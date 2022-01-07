using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_CHECK)]
    public class CiCheckCommand : AbstractConfiguratorCommand
    {
        public CiCheckCommand(ConfiguratorRepository rep) : base(rep)
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
            return $"The article has not been written yet";
        }
    }
}

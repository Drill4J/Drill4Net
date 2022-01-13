using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_LIST)]
    public class CiListCommand : AbstractConfiguratorCommand
    {
        public CiListCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var dir = _rep.GetCiDirectory();
            _cmdHelper.ListConfigs<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir);
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return $"Get list of the {CoreConstants.SUBSYSTEM_CI} configs.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_ACTIVE)]
    public class CiActiveCommand : AbstractConfiguratorCommand
    {
        public CiActiveCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if(_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.ActivateConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir, _desc);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Activate the specified {CoreConstants.SUBSYSTEM_CI} config.";
        }

        public override string GetHelp()
        {
            return HelpHelper.GetActiveConfigText(CoreConstants.SUBSYSTEM_CI, false, RawContexts, "ci");
        }
    }
}

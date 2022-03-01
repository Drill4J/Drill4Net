using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CFG, ConfiguratorConstants.COMMAND_OPEN)]
    public class CfgOpenCommand : AbstractConfiguratorCommand
    {
        public CfgOpenCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var cfgPath = _rep.GetConfiguratorConfigPath();
            var res = _cmdHelper.OpenFile(cfgPath);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Open in external editor the config for the {CoreConstants.SUBSYSTEM_CONFIGURATOR}.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}

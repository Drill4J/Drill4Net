using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using System.IO;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_OPEN)]
    public class SysOpenCommand : AbstractConfiguratorCommand
    {
        public SysOpenCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            RaiseMessage("Be careful! The system properties are stored in three configs: for Agent and Transmitter components, and Injector app.", CliMessageType.Info);
            //
            var agentCfgPath = _rep.GetAgentModelConfigPath();
            var res = File.Exists(agentCfgPath) ? _cmdHelper.OpenFile(agentCfgPath) : false;
            //
            var trCfgPath = _rep.GetTransmitterConfigPath();
            var res2 = File.Exists(trCfgPath) ? _cmdHelper.OpenFile(trCfgPath) : false;
            //
            var injCfgPath = _rep.GetInjectorAppOptionsPath();
            var res3 = File.Exists(injCfgPath) ? _cmdHelper.OpenFile(injCfgPath) : false;
            //
            return Task.FromResult((res && res2 && res3, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return "Open the system configs (connections, etc) in external editor.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

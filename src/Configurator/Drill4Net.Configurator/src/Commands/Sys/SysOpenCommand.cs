using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_OPEN)]
    public class SysOpenCommand : AbstractConfiguratorCommand
    {
        public SysOpenCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            RaiseMessage("Be careful! The system properties are stored in two configs: for Agent and Transmitter components.", CliMessageType.Info);
            //
            var agentCfgPath = _rep.GetAgentConfigPath();
            var res = _cmdHelper.OpenFile(agentCfgPath);
            //
            var trCfgPath = _rep.GetTransmitterConfigPath();
            var res2 = _cmdHelper.OpenFile(trCfgPath);
            //
            return Task.FromResult(res && res2);
        }

        public override string GetShortDescription()
        {
            return "Open in external editor the configs with system properties (connections, etc).";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

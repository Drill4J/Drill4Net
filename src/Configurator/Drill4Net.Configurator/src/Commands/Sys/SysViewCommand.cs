using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_VIEW)]
    public class SysViewCommand : AbstractConfiguratorCommand
    {
        public SysViewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var agCfgPath = _rep.GetAgentModelConfigPath();
            var agentOpts = _rep.ReadAgentOptions(agCfgPath);

            var transCfgPath = _rep.GetTransmitterConfigPath();
            var transOpts = _rep.ReadMessagerOptions(transCfgPath);

            RaiseMessage($"\nDrill service address: {agentOpts.Admin.Url}");
            RaiseMessage($"CreateManualSession: {agentOpts.CreateManualSession}");
            RaiseMessage($"Middleware (Kafka): {string.Join(", ", transOpts.Servers)}");
            _cmdHelper.ViewLogOptions(transOpts.Logs);
            //
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "View the basic system properties (connections, etc).";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

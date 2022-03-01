using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_VIEW)]
    public class SysViewCommand : AbstractConfiguratorCommand
    {
        public SysViewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var agCfgPath = _rep.GetAgentModelConfigPath();
            var agentOpts = _rep.ReadAgentOptions(agCfgPath);

            var transCfgPath = _rep.GetTransmitterConfigPath();
            var transOpts = _rep.ReadMessagerOptions(transCfgPath);

            var injOpts = _rep.ReadInjectorAppOptions();

            RaiseMessage($"\nDrill service address: {agentOpts.Admin.Url}");
            RaiseMessage($"CreateManualSession: {agentOpts.CreateManualSession}");
            RaiseMessage($"Middleware (Kafka): {string.Join(", ", transOpts.Servers)}");
            RaiseMessage($"Injector plugin dir: {string.Join(", ", injOpts.PluginDir)}");
            RaiseMessage($"Agent plugin dir: {string.Join(", ", agentOpts.PluginDir)}");
            _cmdHelper.ViewLogOptions(transOpts.Logs);
            //
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return "View the basic system properties (connections, etc).";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}

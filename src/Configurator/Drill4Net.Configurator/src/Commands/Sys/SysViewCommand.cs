using System.Collections.Generic;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Configuration;

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
            var agCfgPath = _rep.GetAgentConfigPath();
            var agentOpts = _rep.ReadAgentOptions(agCfgPath);

            RaiseMessage($"\n*** Drill service ***");
            RaiseMessage($"Description: {agentOpts.Description}");
            RaiseMessage($"Address: {agentOpts.Admin.Url}");
            RaiseMessage($"Transmitter plugins (contexters): {agentOpts.PluginDir}");
            RaiseMessage($"CreateManualSession: {agentOpts.CreateManualSession}");
            ViewLogOptions(agentOpts.Logs);

            RaiseMessage($"\n*** Target ***");
            RaiseWarning("The empty values for this section are normal");
            RaiseMessage($"Target: {agentOpts.Target.Name}");
            RaiseMessage($"Version: {agentOpts.Target.Version}");
            RaiseMessage($"VersionAssemblyName: {agentOpts.Target.VersionAssemblyName}");
            RaiseMessage($"Tree path: {agentOpts.TreePath}");
            //
            var transCfgPath = _rep.GetTransmitterConfigPath();
            var transOpts = _rep.ReadMessagerOptions(transCfgPath);

            RaiseMessage($"\n*** Middleware ***");
            RaiseMessage($"Description: {transOpts.Description}");
            RaiseMessage($"Address: {string.Join(", ", transOpts.Servers)}");
            ViewLogOptions(transOpts.Logs);
            //
            return Task.FromResult(true);
        }

        private void ViewLogOptions(List<LogData> logs)
        {
            if(logs == null || logs.Count == 0)
                return;
            RaiseMessage($"Additional logs:");
            foreach(LogData logData in logs)
                RaiseMessage($"  -- {logData}");
        }

        public override string GetShortDescription()
        {
            return "View the basic system properties (connections, etc)";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}

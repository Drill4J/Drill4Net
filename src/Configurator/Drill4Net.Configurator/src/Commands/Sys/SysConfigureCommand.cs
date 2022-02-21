using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.CONTEXT_CFG)]
    public class SysConfigureCommand : AbstractConfiguratorCommand
    {
        public SysConfigureCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            _logger.Info("Start to system configure");

            SystemConfiguration cfg = new();
            if (!ConfigAdmin(_rep.Options, cfg))
                return Task.FromResult(FalseEmptyResult);
            if (!ConfigMiddleware(cfg))
                return Task.FromResult(FalseEmptyResult);

            //need to save?
            if(!_cli.AskQuestionBoolean("\nSave the system configuration?", out var yes, true))
                return Task.FromResult(FalseEmptyResult);
            if (yes)
            {
                RaiseMessage("YES", CliMessageType.EmptyInput);
                _rep.SaveSystemConfiguration(cfg); //data are located in the "model configs"
                RaiseMessage($"\nSystem options are saved. {ConfiguratorConstants.MESSAGE_PROPERTIES_EDIT_WARNING}", CliMessageType.Info);
            }
            else
            {
                RaiseWarning("NO");
            }
            return Task.FromResult((yes, new Dictionary<string, object>()));
        }

        internal bool ConfigAdmin(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            var modelCfgPath = _rep.GetAgentModelConfigPath();
            var modelAgentOpts = _rep.ReadAgentOptions(modelCfgPath);
            var admin = modelAgentOpts.Admin;

            // host + port from model
            SimpleParseUrl(admin.Url, out var host, out var portS);

            //Drill host
            var def = host;
            do
            {
                if (!_cli.AskQuestion("Drill service host", out host, def))
                    return false;
            }
            while (!_cli.CheckStringAnswer(ref host, "The service host address cannot be empty"));

            // Drill port
            int port;
            def = portS;
            do
            {
                if (!_cli.AskQuestion("Drill service port", out portS, def))
                    return false;
            }
            while (!_cli.CheckIntegerAnswer(portS, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.AdminUrl = $"{host}:{port}";
            _logger.Info($"Admin url: {cfg.AdminUrl}");

            // Injector's plugin dir
            string plugDir = "";
            def = opts.AgentPluginDirectory;
            do
            {
                if (!_cli.AskQuestion("Injector plugin's directory", out plugDir, def))
                    return false;
            }
            while (!_cli.CheckDirectoryAnswer(ref plugDir, true));
            cfg.InjectorPluginDirectory = plugDir;
            _logger.Info($"Injector plugin dir: {plugDir}");

            // agent's plugin dir (for Transmitter in Server/Worker scheme)
            plugDir = "";
            def = opts.AgentPluginDirectory;
            do
            {
                if (!_cli.AskQuestion("Agent plugin's directory", out plugDir, def))
                    return false;
            }
            while (!_cli.CheckDirectoryAnswer(ref plugDir, true));
            cfg.AgentPluginDirectory = plugDir;
            _logger.Info($"Agent plugin dir: {plugDir}");
            //
            return true;
        }

        internal bool ConfigMiddleware(SystemConfiguration cfg)
        {
            var transCfgPath = _rep.GetTransmitterConfigPath();
            var transOpts = _rep.ReadMessagerOptions(transCfgPath);
            SimpleParseUrl(transOpts.Servers.Count > 0 ? transOpts.Servers[0] : "",
                out var host, out var portS);

            // Kafka host
            var def = host;
            do
            {
                if (!_cli.AskQuestion("Kafka host", out host, def))
                    return false;
            }
            while (!_cli.CheckStringAnswer(ref host, "The Kafka host address cannot be empty"));

            // Kafka port
            int port;
            def = portS;
            do
            {
                if (!_cli.AskQuestion("Kafka port", out portS, def))
                    return false;
            }
            while (!_cli.CheckIntegerAnswer(portS, "The Kafka port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.MiddlewareUrl = $"{host}:{port}";
            _logger.Info($"Kafka url: {cfg.MiddlewareUrl}");

            // Logs
            return _cli.AddLogFile(cfg.Logs, "Drill system");
        }

        internal void SimpleParseUrl(string url, out string host, out string port)
        {
            port = ""; host = "";
            var urlAr = url?.Split(':');
            if (urlAr?.Length > 1)
            {
                host = urlAr[0].Trim();
                port = urlAr[1]?.Trim();
                if (!int.TryParse(port, out var _)) //just the check
                    port = "";
            }
        }

        public override string GetShortDescription()
        {
            return "Configures the basic system properties (connections, etc).";
        }

        public override string GetHelp()
        {
            return "The command works with special configs and has no arguments. Just answer a few questions in interactive mode.";
        }
    }
}

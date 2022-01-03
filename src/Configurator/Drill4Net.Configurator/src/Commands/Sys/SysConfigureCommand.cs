using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator.src.Commands.CI
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.CONTEXT_CFG)]
    public class SysConfigureCommand : AbstractInteractiveCommand
    {
        public SysConfigureCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*************************************************************************/

        public override Task<bool> Process()
        {
            _logger.Info("Start to system configure");

            SystemConfiguration cfg = new();
            if (!ConfigAdmin(_rep.Options, cfg))
                return Task.FromResult(false);
            if (!ConfigMiddleware(_rep.Options, cfg))
                return Task.FromResult(false);

            //TODO: view list of all properties

            //need to save?
            RaiseQuestion("\nSave the system configuration? [y]:");
            var answer = Console.ReadLine().Trim();
            var yes = IsYes(answer);
            if (yes)
            {
                RaiseMessage("YES", CliMessageType.EmptyInput);
                _rep.SaveSystemConfiguration(cfg);
                RaiseMessage($"\nSystem options are saved. {ConfiguratorConstants.MESSAGE_PROPERTIES_EDIT_WARNING}", CliMessageType.Info);
            }
            else
            {
                RaiseWarning("NO");
            }
            return Task.FromResult(yes);
        }

        internal bool ConfigAdmin(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            //Drill host
            string host = "";
            var def = opts.AdminHost;
            do
            {
                if (!AskQuestion("Drill service host", out host, def))
                    return false;
            }
            while (!CheckStringAnswer(ref host, "The service host address cannot be empty"));

            // Drill port
            int port;
            def = opts.AdminPort.ToString();
            string portS;
            do
            {
                if (!AskQuestion("Drill service port", out portS, def))
                    return false;
            }
            while (!CheckIntegerAnswer(portS, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.AdminUrl = $"{host}:{port}";
            _logger.Info($"Admin url: {cfg.AdminUrl }");

            // agent's plugin dir
            string plugDir = "";
            def = opts.PluginDirectory;
            do
            {
                if (!AskQuestion("Agent plugin directory", out plugDir, def))
                    return false;
            }
            while (!CheckDirectoryAnswer(ref plugDir, true));
            cfg.AgentPluginDirectory = plugDir;
            _logger.Info($"Plugin dir: {plugDir}");
            //
            return true;
        }

        internal bool ConfigMiddleware(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            // Kafka host
            string host = "";
            var def = opts.MiddlewareHost;
            do
            {
                if (!AskQuestion("Kafka host", out host, def))
                    return false;
            }
            while (!CheckStringAnswer(ref host, "The Kafka host address cannot be empty"));

            // Kafka port
            int port;
            def = opts.MiddlewarePort.ToString();
            string portS;
            do
            {
                if (!AskQuestion("Kafka port", out portS, def))
                    return false;
            }
            while (!CheckIntegerAnswer(portS, "The Kafka port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.MiddlewareUrl = $"{host}:{port}";
            _logger.Info($"Kafka url: {cfg.MiddlewareUrl}");

            // Logs
            if (!AddLogFile(cfg.Logs, "Drill system"))
                return false;

            return true;
        }
    }
}

using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CFG, ConfiguratorConstants.COMMAND_RESTORE)]
    public class CfgRestoreCommand : AbstractConfiguratorCommand
    {
        public CfgRestoreCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var connNeed = true;
            var pathsNeed = true;
            var force = IsSwitchSet(CoreConstants.SWITCH_FORCE);

            if (!force)
            {
                if (!_cli.AskQuestion($"Do you really want to restore the default configuration settings for the {CoreConstants.SUBSYSTEM_CONFIGURATOR}?",
                    out var answer, "n"))
                    return Task.FromResult(false);
                if (_cli.IsYes(answer))
                    return Task.FromResult(false);
                //
                if (!_cli.AskQuestion("To restore the 'default connections' settings?", out answer, "n"))
                    return Task.FromResult(false);
                connNeed = _cli.IsYes(answer);
                //
                if (!_cli.AskQuestion("To restore the paths of the applications and components?\nThis can be done only if all folders are in their standard places, as it was in the distribution.",
                    out answer, "n"))
                    return Task.FromResult(false);
                pathsNeed = _cli.IsYes(answer);
            }
            
            // set up
            var opts = _rep.Options;
            opts.ExternalEditor = null;
            opts.ProjectsDirectory = null;
            opts.Logs = null;
            if (connNeed) //it is located in the "model configs"
            {
                _rep.SetDefaultSystemConfiguration();
            }
            if (pathsNeed) //Configurator's options
            {
                opts.InstallDirectory = ConfiguratorConstants.PATH_INSTALL;
                opts.InjectorDirectory = ConfiguratorConstants.PATH_INJECTOR;
                opts.TestRunnerDirectory = ConfiguratorConstants.PATH_RUNNER;
                opts.CiDirectory = ConfiguratorConstants.PATH_CI;
                opts.TransmitterDirectory = ConfiguratorConstants.PATH_TRANSMITTER;
                opts.PluginDirectory = ConfiguratorConstants.PATH_TRANSMITTER_PLUGINS; //Agent's plugins (IEngineContexters)
            }

            //save
            _rep.WriteConfiguratorOptions(opts);
            RaiseMessage("The settings are restored.", CliMessageType.Info);

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Restores defaults for the {CoreConstants.SUBSYSTEM_CONFIGURATOR} itself.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

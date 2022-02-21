using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CFG, ConfiguratorConstants.COMMAND_RESTORE)]
    public class CfgRestoreCommand : AbstractConfiguratorCommand
    {
        public CfgRestoreCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var connNeed = true;
            var pathsNeed = true;
            var force = IsSwitchSet(CoreConstants.SWITCH_FORCE);

            if (!force)
            {
                if (!_cli.AskQuestionBoolean($"Do you really want to restore the default configuration settings for the {CoreConstants.SUBSYSTEM_CONFIGURATOR}?",
                    out var answerBool, false))
                    return Task.FromResult(FalseEmptyResult);
                if (!answerBool)
                    return Task.FromResult(FalseEmptyResult);
                //
                if (!_cli.AskQuestionBoolean("To restore the 'default connections' settings?", out connNeed, false))
                    return Task.FromResult(FalseEmptyResult);
                if (!_cli.AskQuestionBoolean("To restore the paths of the applications and components?\nThis can be done only if all folders are in their standard places, as it was in the distribution",
                    out pathsNeed, false))
                    return Task.FromResult(FalseEmptyResult);
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
                opts.AgentPluginDirectory = ConfiguratorConstants.PATH_TRANSMITTER_PLUGINS; //Agent's plugins (IEngineContexters)
            }

            //save Configurator app itself
            _rep.WriteConfiguratorOptions(opts);

            //save options for the Injector app
            var injOpts = _rep.ReadInjectorAppOptions();
            injOpts.PluginDir = ConfiguratorConstants.PATH_INJECTOR_PLUGINS; //IInjectorPlugin
            _rep.WriteInjectorAppOptions(injOpts);

            RaiseMessage("The settings are restored.", CliMessageType.Info);

            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return $"Restores defaults for the {CoreConstants.SUBSYSTEM_CONFIGURATOR} itself.";
        }

        public override string GetHelp()
        {
            return @$"The command uses the config file of the Configurator itself. Now some values in it can be changed only by editing the file directly (for example, the system editor path for commands like ""{ConfiguratorConstants.CONTEXT_RUNNER} {ConfiguratorConstants.COMMAND_OPEN}"").
After executing the command, the config will contain default values (in the case of Windows, Notepad will become the config editor).";
        }
    }
}

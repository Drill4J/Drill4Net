using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_PREP)]
    public class TargetPrepCommand : AbstractConfiguratorCommand
    {
        public TargetPrepCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var injCfg = GetPositional(0); //cfg name
            var injDir = GetParameter(CoreConstants.ARGUMENT_DESTINATION_DIR, false); //injected target dir
            string? err;
            if (!string.IsNullOrWhiteSpace(injCfg))
            {
                if (!Path.HasExtension(injCfg))
                    injCfg += ".yml";

                var cfgPath = string.IsNullOrWhiteSpace(Path.GetPathRoot(injCfg)) ? //is it full path?
                    Path.Combine(_rep.GetInjectorDirectory(), injCfg) : //local config for the Injector
                    injCfg;

                if(!File.Exists(cfgPath))
                {
                    err = $"{CoreConstants.SUBSYSTEM_INJECTOR}'s config does not exist: [{cfgPath}]";
                    _logger.Error(err);
                    RaiseError(err);
                    return Task.FromResult(false);
                }
                try
                {
                    var opts = _rep.ReadInjectorOptions(cfgPath, true); //it needs to be processed to get the destination path
                    return Task.FromResult(CheckInjectedTarget(opts.Destination.Directory));
                }
                catch(Exception ex)
                {
                    err = $"The {CoreConstants.SUBSYSTEM_INJECTOR}'s config cannot be read: [{cfgPath}]";
                    _logger.Error(err, ex);
                    RaiseError(err);
                    return Task.FromResult(false);
                }
            }
            else
            if (!string.IsNullOrWhiteSpace(injDir))
            {
                return Task.FromResult(CheckInjectedTarget(injDir));
            }
            else
            {
                err = "You have to specify either the name of the config or the folder with the instrumented target.";
                _logger.Error(err);
                RaiseError(err);
                return Task.FromResult(false);
            }
        }

        internal bool CheckInjectedTarget(string dir)
        {
            string? err;
            if (!Directory.Exists(dir))
            {
                err = $"Directory does not exist: [{dir}]. Perform the target injection first by specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config.";
                _logger.Error(err);
                RaiseError(err);
                return false;
            }
            //
            var trCfgName = CoreConstants.CONFIG_NAME_DEFAULT; //Transmitter's config (it has default name)
            var trCfgPath = Path.Combine(dir, trCfgName);
            if (!File.Exists(trCfgPath))
            {
                var modelCfgPath = Path.Combine(_rep.GetInstallDirectory(), trCfgName);
                if (!File.Exists(modelCfgPath))
                {
                    RaiseError($"Model {CoreConstants.SUBSYSTEM_TRANSMITTER}'s config not found: [{modelCfgPath}]");
                    return false;
                }
                try
                {
                    //check cfg itself
                    var opts = _rep.ReadAgentOptions(modelCfgPath);
                    //perhaps later there will be additional configuration settings for a specific target
                    _rep.WriteAgentOptions(opts, trCfgPath);
                    RaiseMessage($"{CoreConstants.SUBSYSTEM_AGENT}'s config is written to the target directory: [{trCfgPath}]", CliMessageType.Info);
                }
                catch (Exception ex)
                {
                    err = $"Default agent's config exist in install directory but cannot be read: [{modelCfgPath}]";
                    _logger?.Error(err, ex);
                    RaiseError(err);
                }
            }
            else
            {
                try
                {
                    //check cfg itself
                    var opts = _rep.ReadAgentOptions(trCfgPath);
                    //here we just check the fact of reading the config
                    RaiseMessage($"{CoreConstants.SUBSYSTEM_AGENT}'s config is ok: [{trCfgPath}]", CliMessageType.Info);
                }
                catch (Exception ex)
                {
                    err = $"{CoreConstants.SUBSYSTEM_AGENT}'s config exist in target directory but cannot be read: [{trCfgPath}]";
                    _logger?.Error(err, ex);
                    RaiseError(err);
                }
            }

            return true;
        }

        public override string GetShortDescription()
        {
            return "Prepare the injected target to run it";
        }

        public override string GetHelp()
        {
            return @$"Check and prepare the injected target for additional requirements, it is now the presence of an {CoreConstants.SUBSYSTEM_AGENT}'s config. If necessary, such a config is created using a model file, which, in turn, is configured by system settings.

You can to do it by passing the short name of config file (local for Injector) or full path to it:
    Example: trg prep -- cfg2
    Example: trg prep -- ""d:\Targets\TargA.Injected\cfg2.yml""

Another option is passing the injected target directory directly using ""dest_dir"" option:
    Example: trg prep --dest_dir=""d:\Targets\TargA.Injected\""";
        }
    }
}

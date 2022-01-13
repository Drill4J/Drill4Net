using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_DELETE)]
    public class CiDeleteCommand : AbstractConfiguratorCommand
    {
        public CiDeleteCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);

            bool res = false;
            var doNotDeleteIntegration = IsSwitchSet(ConfiguratorConstants.SWITCH_INTEGRATION_NO);
            var deleteOnlyIntegration = IsSwitchSet(ConfiguratorConstants.SWITCH_INTEGRATION);

            //delete the config
            var cfgPath = GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH, false);
            if (string.IsNullOrWhiteSpace(cfgPath))
            {
                var cfgDir = _rep.GetCiDirectory();
                if (deleteOnlyIntegration)
                {
                    res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath,
                        out var _, out var error);
                    if (!res)
                        RaiseError(error);
                }
                else
                {
                    res = _cmdHelper.DeleteConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath);
                }
                if (!res)
                    return Task.FromResult(FalseEmptyResult);
            }

            //delete the integrations with IDE
            if (!doNotDeleteIntegration)
            {
                var ide = new IdeConfigurator(_rep);
                var solutionDir = GetParameter(CoreConstants.ARGUMENT_SOURCE_DIR, false);
                if (string.IsNullOrWhiteSpace(solutionDir))
                {
                    if (!_cli.AskDirectory(ConfiguratorConstants.MESSAGE_CI_INTEGRATION_IDE_DIR,
                        out solutionDir, ide.GetDefaultProjectSourcesDirectory(), true))
                        return Task.FromResult(TrueEmptyResult);
                }
                res = ide.DeleteInjections(solutionDir, cfgPath, out var processed, out var errors, out var all);
                //
                var cnt = processed.Count + errors.Count;
                RaiseMessage($"\nProcessed: {processed.Count}/{cnt} (total: {all})");
                foreach (var prc in processed)
                    RaiseMessage(prc, CliMessageType.Info);

                foreach (var error in errors)
                        RaiseError($"{error.path} -> {error.error}");
            }
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_CI} config and its integrations (if possible).";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

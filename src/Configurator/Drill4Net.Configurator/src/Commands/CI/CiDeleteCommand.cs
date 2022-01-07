using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_DELETE)]
    public class CiDeleteCommand : AbstractConfiguratorCommand
    {
        public CiDeleteCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);

            bool res = false;
            var doNotDeleteIntegration = IsSwitchSet('I');
            var deleteOnlyIntegration = IsSwitchSet('i');

            //delete the config
            var cfgPath = GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH, false);
            if (string.IsNullOrWhiteSpace(cfgPath))
            {
                var cfgDir = _rep.GetCiDirectory();
                if (deleteOnlyIntegration)
                {
                    res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath,
                        out var _, out var error);
                }
                else
                {
                    res = _cmdHelper.DeleteConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath);
                }
                if (!res)
                    return Task.FromResult(false);
            }

            //delete the integrations with IDE
            if (!doNotDeleteIntegration)
            {
                var ide = new IdeConfigurator(_rep);
                var solutionDir = GetParameter(CoreConstants.ARGUMENT_SOURCE_DIR, false);
                if (string.IsNullOrWhiteSpace(solutionDir))
                {
                    if (!_cli.AskDirectory("", out solutionDir, ide.GetDefaultProjectSourcesDirectory(), true))
                        return Task.FromResult(true);
                }
                res = ide.DeleteInjections(solutionDir, cfgPath);
            }
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_CI}'s config";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

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

            //delete the config
            var cfgDir = _rep.GetCiDirectory();
            var cfgPath = GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH, false);

            bool res;
            if (string.IsNullOrWhiteSpace(cfgPath))
            {
                res = _cmdHelper.DeleteConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, cfgDir, _desc, out cfgPath);
                if (!res)
                    return Task.FromResult(false);
            }

            //delete the integrations with IDE
            var ide = new IdeConfigurator(_rep);
            var solutionDir = GetParameter(CoreConstants.ARGUMENT_SOURCE_DIR, false);
            if (string.IsNullOrWhiteSpace(solutionDir))
            {
                if (!_cli.AskDirectory("", out solutionDir, ide.GetDefaultProjectSourcesDirectory(), true))
                    return Task.FromResult(true);
            }
            res = ide.DeleteInjections(solutionDir, cfgPath);

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

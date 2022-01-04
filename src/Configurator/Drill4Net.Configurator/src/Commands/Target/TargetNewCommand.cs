using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_NEW)]
    public class TargetNewCommand : AbstractTargetEditor
    {
        public TargetNewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var modelCfgPath = Path.Combine(_rep.Options.InstallDirectory, ConfiguratorConstants.CONFIG_INJECTOR_MODEL);
            if (!File.Exists(modelCfgPath))
            {
                RaiseError($"Model Injector's config not found: [{modelCfgPath}]");
                return Task.FromResult(false);
            }

            var res = Edit(modelCfgPath, true);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return "Create new Injector's config in interactive mode";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}

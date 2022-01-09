using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_NEW)]
    public class TargetNewCommand : AbstractTargetEditor
    {
        public TargetNewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var modelCfgPath = Path.Combine(_rep.GetInstallDirectory(), ConfiguratorConstants.CONFIG_INJECTOR_MODEL);
            if (!File.Exists(modelCfgPath))
            {
                RaiseError($"Model {CoreConstants.SUBSYSTEM_INJECTOR}'s config not found: [{modelCfgPath}]");
                return Task.FromResult(false);
            }

            var res = Edit(modelCfgPath, true);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Create new {CoreConstants.SUBSYSTEM_INJECTOR}'s config in interactive mode.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

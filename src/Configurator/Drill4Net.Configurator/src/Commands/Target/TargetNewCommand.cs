using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_NEW)]
    public class TargetNewCommand : AbstractTargetEditor
    {
        public TargetNewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var modelCfgPath = _rep.GetInjectorModelConfigPath();
            if (!File.Exists(modelCfgPath))
            {
                RaiseError($"Model {CoreConstants.SUBSYSTEM_INJECTOR} config not found: [{modelCfgPath}]");
                return Task.FromResult(FalseEmptyResult);
            }

            var res = Edit(modelCfgPath, true);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Create new {CoreConstants.SUBSYSTEM_INJECTOR} config in interactive mode.";
        }

        public override string GetHelp()
        {
            return @"This command allows you to interactively create a configuration from scratch for the injection of the target application (SUT - system under test). You should simply answer a number of clarifying questions. This is the first stage in the full workflow of the Drill for .NET.

The command does not accept any clarifying arguments yet, but this may be done in the near future.

    Example: trg new";
        }
    }
}

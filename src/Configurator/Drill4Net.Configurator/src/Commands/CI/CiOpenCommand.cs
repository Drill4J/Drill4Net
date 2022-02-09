using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_OPEN)]
    public class CiOpenCommand : AbstractConfiguratorCommand
    {
        public CiOpenCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.OpenConfig<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir, _desc);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Open the config for {CoreConstants.SUBSYSTEM_CI}'s in external editor.";
        }

        public override string GetHelp()
        {
            return @$"{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_CI, RawContexts, "ci")}";
        }
    }
}

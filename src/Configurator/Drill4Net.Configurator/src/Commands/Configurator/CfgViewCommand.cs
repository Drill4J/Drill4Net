using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator.src.Commands.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CFG, ConfiguratorConstants.COMMAND_VIEW)]
    public class CfgViewCommand : AbstractConfiguratorCommand
    {
        public CfgViewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var cfgPath = _rep.GetInjectorAppOptionsPath();
            var done = _cmdHelper.ViewFile(cfgPath);
            return Task.FromResult((done, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"View the content of {CoreConstants.SUBSYSTEM_INJECTOR} app config.";
        }

        public override string GetHelp()
        {
            return @$" Example: {RawContexts}";
        }
    }
}

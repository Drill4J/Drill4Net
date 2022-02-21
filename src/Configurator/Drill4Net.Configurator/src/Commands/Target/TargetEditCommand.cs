using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TargetEditCommand : AbstractTargetEditor
    {
        public TargetEditCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);

            var dir = _rep.GetInjectorDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfigPath<InjectionOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc, out var sourcePath,
                out var _, out var error);
            if (!res)
            {
                if(string.IsNullOrWhiteSpace(error))
                    RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            res = Edit(sourcePath, false);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Edit the specified {CoreConstants.SUBSYSTEM_INJECTOR} config in interactive mode.";
        }

        public override string GetHelp()
        {
            return @$"{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts, "targetA")}";
        }
    }
}

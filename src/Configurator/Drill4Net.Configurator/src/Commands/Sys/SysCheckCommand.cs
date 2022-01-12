using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_CHECK)]
    public class SysCheckCommand : AbstractConfiguratorCommand
    {
        public SysCheckCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            //var injCfg = GetPositional(0); //cfg name
            //var injDir = GetParameter(CoreConstants.ARGUMENT_SOURCE_DIR, false); //injected target dir
            //if (string.IsNullOrWhiteSpace(injDir) && string.IsNullOrWhiteSpace(injCfg))
            //{
            //    RaiseError("You have to specify either the name of the config or the folder with the instrumented target.");
            //    return Task.FromResult(false);
            //}
            ////

            //
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return "";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

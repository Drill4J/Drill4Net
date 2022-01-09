using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator.src.Commands.Sys
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_CHECK)]
    public class SysCheckCommand : AbstractConfiguratorCommand
    {
        public SysCheckCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
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
            return Task.FromResult(true);
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

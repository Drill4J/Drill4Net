using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_ABOUT)]
    public class AboutCommand : AbstractConfiguratorCommand
    {
        public AboutCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            const string? mess = @"There will be a description of the program here someday...
";
            RaiseMessage(mess, CliMessageType.Help);
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return "About the Program, its command line interface, main workflow, etc.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}

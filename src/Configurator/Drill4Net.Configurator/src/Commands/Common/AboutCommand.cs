using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_ABOUT)]
    public class AboutCommand : AbstractConfiguratorCommand
    {
        public AboutCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            const string? mess = @"There will be a description of the program here someday...
";
            RaiseMessage(mess, CliMessageType.Help);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "About the Programm, its command line interface, main workflow, etc.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}

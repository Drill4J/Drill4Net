using System.Collections.Generic;

namespace Drill4Net.Cli
{
    [CliCommandAttribute(CliConstants.COMMAND_NULL)]
    public class NullCliCommand : AbstractCliCommand
    {
        public NullCliCommand(string subsystem) : base(subsystem)
        {
        }

        /***************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            RaiseError("The command is not found");
            return Task.FromResult(FalseEmptyResult);
        }

        public override string GetHelp()
        {
            return "The dummy empty object is not used directly";
        }

        public override string GetShortDescription()
        {
            return "The dummy empty object when the specified command is not found";
        }
    }
}

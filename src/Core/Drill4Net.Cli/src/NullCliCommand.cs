namespace Drill4Net.Cli
{
    [CliCommandAttribute(CliConstants.COMMAND_NULL)]
    public class NullCliCommand : AbstractCliCommand
    {
        public override Task<bool> Process()
        {
            RaiseError("The command is not found");
            return Task.FromResult(false);
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

namespace Drill4Net.Cli
{
    [CliCommandAttribute("NULL")]
    public class NullCliCommand : AbstractCliCommand
    {
        public override Task<bool> Process()
        {
            RaiseError("The command is not found");
            return Task.FromResult(false);
        }
    }
}

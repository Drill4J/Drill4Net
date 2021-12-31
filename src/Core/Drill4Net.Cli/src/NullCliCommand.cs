namespace Drill4Net.Cli
{
    public class NullCliCommand : AbstractCliCommand
    {
        public override Task<bool> Process()
        {
            RaiseError("The command is dummy null object", true);
            return Task.FromResult(false);
        }
    }
}

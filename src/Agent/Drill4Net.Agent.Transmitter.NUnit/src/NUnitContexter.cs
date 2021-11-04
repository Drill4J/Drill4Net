using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transmitter.NUnit
{
    public class NUnitContexter : AbstractContexter
    {
        public NUnitContexter() : base(nameof(NUnitContexter))
        {
        }

        /******************************************************************/

        public override string GetContextId()
        {
            return null; //TODO real !!!!
        }

        public override void RegisterCommand(int command, string data)
        {
            //nothing
        }
    }
}

using Drill4Net.Common;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transmitter.NUnit3
{
    //https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html

    public class NUnitContexter : AbstractEngineContexter
    {
        public NUnitContexter() : base(nameof(NUnitContexter))
        {
        }

        /******************************************************************/

        public override string GetContextId()
        {
            return null; // NUnit.Framework.TestContext.CurrentContext?.Test?.FullName; //TODO: check !!!!
        }

        public override TestEngine GetTestEngine()
        {
            return new TestEngine
            {
                Name = "NUnit",
                Version = FileUtils.GetProductVersion(typeof(NUnit.Framework.TestContext)),
                MustSequential = false,
            };
        }

        public override bool RegisterCommand(int command, string data)
        {
            return true;
        }
    }
}

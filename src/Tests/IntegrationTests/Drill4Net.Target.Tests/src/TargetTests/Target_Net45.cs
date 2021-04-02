using System.Collections.Generic;

namespace Drill4Net.Target.Tests
{
    internal class Target_Net45 : AbstractInjectTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_NET45);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET45);
        }
    }
}

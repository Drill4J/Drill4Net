using System.Collections.Generic;

namespace Drill4Net.Target.Tests
{
    internal class Target_Core22 : AbstractInjectTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_CORE22);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_CORE22);
        }
    }
}

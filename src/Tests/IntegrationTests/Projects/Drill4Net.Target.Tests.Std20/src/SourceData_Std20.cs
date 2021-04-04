using System.Collections.Generic;
using Drill4Net.Target.Tests.Common;

namespace Drill4Net.Target.Tests.Std20
{
    public class SourceData_Std20 : AbstractTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_NET50);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET50);
        }
    }
}
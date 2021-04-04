using System.Collections.Generic;
using Drill4Net.Target.Tests.Common;
using NUnit.Framework;

namespace Drill4Net.Target.Tests.Std20
{
    [TestFixture]
    public class SourceData_Std20 : AbstractTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_STANDARD20);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_STANDARD20);
        }
    }
}
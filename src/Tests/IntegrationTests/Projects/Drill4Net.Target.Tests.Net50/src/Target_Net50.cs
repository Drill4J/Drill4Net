using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Target.Tests.Common;

namespace Drill4Net.Target.Tests.Net50
{
    [TestFixture]
    internal class SourceData_Net50 : AbstractTargetTests
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
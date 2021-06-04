using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Target.Tests.Common;

namespace Drill4Net.Target.Tests.Net461
{
    [TestFixture]
    internal class Target_Net461 : AbstractTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_NET461);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET461);
        }
    }
}

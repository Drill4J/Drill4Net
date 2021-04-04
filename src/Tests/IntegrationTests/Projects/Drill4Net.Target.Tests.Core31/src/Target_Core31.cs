using System.Collections.Generic;
using Drill4Net.Target.Tests.Common;
using NUnit.Framework;

namespace Drill4Net.Target.Tests.Core31
{
    [TestFixture]
    internal class Target_Core31 : AbstractTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_CORE31);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_CORE31);
        }
    }
}

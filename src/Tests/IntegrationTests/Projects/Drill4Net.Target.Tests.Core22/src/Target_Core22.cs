using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Target.Tests.Common;

namespace Drill4Net.Target.Tests.Core22
{
    [TestFixture]
    internal class Target_Core22 : AbstractTargetTests
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

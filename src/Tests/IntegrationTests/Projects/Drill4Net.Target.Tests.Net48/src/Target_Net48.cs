using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Target.Tests.Common;

namespace Drill4Net.Target.Tests.Net48
{
    //[TestFixture]
    internal class Target_Net48 : AbstractTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_NET48);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET48);
        }
    }
}

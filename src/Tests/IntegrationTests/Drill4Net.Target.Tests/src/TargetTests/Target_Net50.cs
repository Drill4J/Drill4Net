using System.Collections.Generic;
using NUnit.Framework;

namespace Drill4Net.Target.Tests
{
    internal class Target_Net50 : AbstractInjectTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTarget(TestConstants.MONIKER_NET50);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET50);
        }

        /******************************************************************/

        [TestCaseSource(typeof(SourceData_Net50), "Net50_Simple")]
        public void Net50_Simple(string methodName, object[] args, List<string> check)
        {
            Base_Simple(methodName, args, check);
        }

        [TestCaseSource(typeof(SourceData_Net50), "Net50_Parented")]
        public void Net50_Parented(object[] args, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] inputs)
        {
            Base_Parented(args, isAsync, isBunch, ignoreEnterReturns, inputs);
        }
    }
}

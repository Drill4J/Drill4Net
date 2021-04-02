using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Drill4Net.Target.Tests
{
    internal class Target_Net50 : AbstractInjectTargetTests
    {
        protected override Dictionary<string, object> LoadTarget()
        {
            return _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_NET50);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET50);
        }

        /******************************************************************/

        [TestCaseSource(typeof(SourceData_Common), "Net50_Simple")]
        public void Net50_Simple(object target, MethodInfo mi, object[] args, List<string> checks)
        {
            
        }

        [TestCaseSource(typeof(SourceData_Common), "Net50_Parented")]
        public void Net50_Parented(object target, object[] args, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] inputs)
        {
            
        }
    }
}

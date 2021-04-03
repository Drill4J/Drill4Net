using System.Collections.Generic;
using System.Reflection;
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

        //[TestCaseSource(typeof(SourceData_Net50), "Net50_Simple")]
        //public void Net50_Simple(MethodInfo mi, object[] args, List<string> check)
        //{
        //    Base_Simple(mi, args, check);
        //}

        //[TestCaseSource(typeof(SourceData_Net50), "Net50_Parented")]
        //public void Net50_Parented(MethodInfo mi, object[] args, bool isAsync, bool isBunch, 
        //    bool ignoreEnterReturns, params TestInfo[] inputs)
        //{
        //    Base_Parented(mi, args, isAsync, isBunch, ignoreEnterReturns, inputs);
        //}
    }
}

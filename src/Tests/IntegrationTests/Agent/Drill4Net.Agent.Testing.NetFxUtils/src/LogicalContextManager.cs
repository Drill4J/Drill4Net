using System.Runtime.Remoting.Messaging;
using NUnit.Framework.Internal;

namespace Drill4Net.Agent.Testing.NetFxUtils
{
    /// <summary>
    /// Utils methods for CallContext in NetFramework
    /// </summary>
    public static class LogicalContextManager
    {
        /// <summary>
        /// Get NUnit test's context for current Run
        /// </summary>
        /// <returns></returns>
        public static TestExecutionContext GetNUnitTestContext()
        {
            var ret = CallContext.LogicalGetData("NUnit.Framework.TestExecutionContext");
            return ret as TestExecutionContext;
        }
    }
}

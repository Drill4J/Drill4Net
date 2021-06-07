using NUnit.Framework.Internal;
using System.Runtime.Remoting.Messaging;

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
        public static TestExecutionContext GetTestFromLogicalContext()
        {
            var ret = CallContext.LogicalGetData("NUnit.Framework.TestExecutionContext") as TestExecutionContext;
            return ret;
        }
    }
}

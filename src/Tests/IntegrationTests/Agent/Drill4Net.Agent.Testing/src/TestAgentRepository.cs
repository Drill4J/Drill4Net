using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Testing
{
    /// <summary>
    /// Repository for the Tester subsystem
    /// </summary>
    public class TestAgentRepository : TreeRepository<TesterOptions, BaseOptionsHelper<TesterOptions>>
    {
        /// <summary>
        /// Initializes a new instance of the repository for the Tester subsystem.
        /// </summary>
        /// <param name="cfgPath">The CFG path.</param>
        public TestAgentRepository(string cfgPath = null) : base(CoreConstants.SUBSYSTEM_TESTER, cfgPath)
        {
        }

        /***************************************************************************/

        /// <summary>
        /// Gets the concrete targets' root dir by calling assembly directory.
        /// </summary>
        /// <param name="callingDir">The calling assembly directory.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Base directory for tests is empty. See {CoreConstants.CONFIG_TESTS_NAME}</exception>
        public string GetTargetsDir(string callingDir)
        {
            var baseDir = FindTestsDir(callingDir);
            if (baseDir == null)
                throw new Exception($"Base directory for the tests is empty. See {CoreConstants.CONFIG_NAME_TESTS}");
            if (baseDir.EndsWith("\\"))
                baseDir = baseDir.Remove(baseDir.Length - 1, 1);
            return Path.Combine(baseDir, Options.TreePath);
        }

        /// <summary>
        /// Gets the root directory of the tests by some inner directory into it.
        /// </summary>
        /// <param name="innerDir">The inner dir.</param>
        /// <returns></returns>
        internal string FindTestsDir(string innerDir)
        {
            var targets = Options.Versions.Targets;
            //crunch...
            if (!targets.ContainsKey("netstandard2.0"))
                targets.Add("netstandard2.0", null);
            if (!targets.ContainsKey("netstandard2.1"))
                targets.Add("netstandard2.1", null);
            //
            var di = new DirectoryInfo(innerDir);
            if (targets.ContainsKey(di.Name))
                di = di.Parent;
            return di.Parent.FullName;
        }
    }
}

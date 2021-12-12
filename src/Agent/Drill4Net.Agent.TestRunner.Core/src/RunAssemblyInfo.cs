using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class RunAssemblyInfo
    {
        internal string AssemblyName { get; set; }
        internal bool MustSequential { get; set; }

        /// <summary>
        /// Tests to run (by QulifiedName now)
        /// </summary>
        internal List<string> Tests { get; set; } = new();

        /***********************************************************************/

        public override string ToString()
        {
            return $"{AssemblyName} -> tests: {Tests.Count} -> MustSequential: {MustSequential}";
        }
    }
}

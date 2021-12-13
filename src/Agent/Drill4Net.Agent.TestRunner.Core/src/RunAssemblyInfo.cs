using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class RunAssemblyInfo
    {
        internal string Directory { get; set; }
        internal string AssemblyName { get; set; }
        internal string AssemblyPath => System.IO.Path.Combine(Directory, AssemblyName);
        internal bool MustSequential { get; set; }

        /// <summary>
        /// Tests to run (by QulifiedName now)
        /// </summary>
        internal List<string> Tests { get; set; } = new();

        /***********************************************************************/

        public override string ToString()
        {
            return $"{AssemblyPath} -> tests: {Tests.Count} -> MustSequential: {MustSequential}";
        }
    }
}

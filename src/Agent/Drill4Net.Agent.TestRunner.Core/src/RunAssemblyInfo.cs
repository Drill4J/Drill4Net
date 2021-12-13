using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class RunAssemblyInfo
    {
        /// <summary>
        /// Original tests' assembly directory
        /// </summary>
        internal string OrigDirectory { get; set; }

        /// <summary>
        /// Original tests' assembly path
        /// </summary>
        internal string OrigAssemblyPath => System.IO.Path.Combine(OrigDirectory, AssemblyName);
        
        internal string AssemblyName { get; set; }

        internal bool MustSequential { get; set; }

        /// <summary>
        /// Tests to run (by QulifiedName now)
        /// </summary>
        internal List<string> Tests { get; set; } = new();

        /***********************************************************************/

        public override string ToString()
        {
            return $"{OrigAssemblyPath} -> tests: {Tests.Count} -> MustSequential: {MustSequential}";
        }
    }
}

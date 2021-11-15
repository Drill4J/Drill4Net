using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class RunAssemblyInfo
    {
        internal bool MustSequential { get; set; }

        internal string AssemblyName { get; set; }

        /// <summary>
        /// Tests to run (by QulifiedName now)
        /// </summary>
        internal List<string> Tests { get; set; } = new();
    }
}

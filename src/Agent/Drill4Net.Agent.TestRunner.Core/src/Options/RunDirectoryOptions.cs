using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Options for run test on Directory level, which can contains several test assemblies
    /// </summary>
    [Serializable]
    public class RunDirectoryOptions
    {
        /// <summary>
        /// Directory for the injected tests (full path??)
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Assemblies with the injected tests
        /// </summary>
        public List<RunAssemblyOptions> Assemblies { get; set; }

        /// <summary>
        /// The parallel execution is restricted on current directory level.
        /// Missed value inherits value from the Run level in <see cref="TestRunnerOptions"/>
        /// </summary>
        public bool? DefaultParallelRestrict { get; set; }

        /***********************************************************************/

        public override string ToString()
        {
            return $"{Directory} -> assemblies: {Assemblies.Count}, DefaultParallelRestrict: {DefaultParallelRestrict}";
        }
    }
}

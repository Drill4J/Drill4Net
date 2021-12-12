using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Info about tests to run
    /// </summary>
    internal class RunInfo
    {
        /// <summary>
        /// Type of autotests' run
        /// </summary>
        internal RunningType RunType { get; set; } = RunningType.Unknown;

        /// <summary>
        /// Tests to run with parameters by assembly path
        /// </summary>
        internal Dictionary<string, RunAssemblyInfo> AssemblyInfos { get; set; } = new();

        //TODO: restrict count of simultaneously running cmd processes

        /***********************************************************************/

        public override string ToString()
        {
            return $"{RunType} -> assemblies: {AssemblyInfos.Count}";
        }
    }
}

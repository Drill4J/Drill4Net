using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Info about tests to run in some directory
    /// </summary>
    internal class DirectoryRunInfo
    {
        internal string Target { get; set; }

        /// <summary>
        /// Type of autotests' run
        /// </summary>
        internal RunningType RunType { get; set; } = RunningType.Unknown;

        /// <summary>
        /// Tests to run with parameters by assembly path retrieved
        /// from admin side for WHOLE Target
        /// </summary>
        internal Dictionary<string, RunAssemblyInfo> RunAssemblyInfos { get; set; } = new();

        /// <summary>
        /// Options for tests' directory
        /// </summary>
        internal RunDirectoryOptions DirectoryOptions { get; set; }

        /// <summary>
        ///  Options for tests' assembly
        /// </summary>
        internal RunAssemblyOptions AssemblyOptions { get; set; }

        //TODO: restrict count of simultaneously running cmd processes

        /***********************************************************************/

        public override string ToString()
        {
            return $"{Target}: {RunType} -> [{DirectoryOptions.Path}] -> assemblies: {RunAssemblyInfos.Count}";
        }
    }
}

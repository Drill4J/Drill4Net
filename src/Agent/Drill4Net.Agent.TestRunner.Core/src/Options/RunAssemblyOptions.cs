using System;

namespace Drill4Net.Agent.TestRunner.Core
{
    [Serializable]
    public class RunAssemblyOptions
    {
        /// <summary>
        /// DEFAULT assembly to run tests if no builds still exists in Drill service
        /// </summary>
        public string DefaultAssemblyName { get; set; }

        /// <summary>
        /// The parallel execution is restricted by default for tests inside
        /// (if no builds still exists in Drill service)
        /// </summary>
        public bool DefaultParallelRestrict { get; set; }

        /***********************************************************************/

        public override string ToString()
        {
            return $"{DefaultAssemblyName} -> DefaultParallelRestrict: {DefaultParallelRestrict}";
        }
    }
}

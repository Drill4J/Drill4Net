using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    public class BaseTestContext
    {
        /// <summary>
        /// Assembly file's full path
        /// </summary>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// User tags of the test's context
        /// </summary>
        public List<string> Tags { get; set; }
    }
}
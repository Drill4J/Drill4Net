using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Agent.Abstract.Transfer
{
    /// <summary>
    /// Probes of instrumented assemblies for sending of collecting data to admin side
    /// </summary>
    [Serializable]
    public class ExecClassData
    {
        /// <summary>
        /// Name of the test (session)
        /// </summary>
        public string TestName { get; }
        
        /// <summary>
        /// Can ignore
        /// </summary>
        public long? Id { get; }

        /// <summary>
        /// Full class name
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// List of probe data
        /// </summary>
        public List<bool> Probes { get; private set; }
        
        /**************************************************************************/
        
        public ExecClassData(string testName, string className)
        {
            Id = className.GetHashCode();
            TestName = testName ?? throw new ArgumentNullException(nameof(testName));
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
        }

        /**************************************************************************/

        public void InitProbes(int probeCnt)
        {
            Probes = Enumerable.Repeat(false, probeCnt).ToList();
        }

        public override string ToString()
        {
            return $"{TestName} -> {Id}: {ClassName}";
        }
    }
}

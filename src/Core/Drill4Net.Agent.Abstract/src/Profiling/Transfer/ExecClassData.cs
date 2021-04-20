using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Classes for including to the scope of collecting data
    /// </summary>
    public class ExecClassData
    {
        /// <summary>
        /// Name of the test (session)
        /// </summary>
        public string TestName { get; }
        
        public long? Id { get; }

        /// <summary>
        /// Full class name
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// List of probe data
        /// </summary>
        public List<bool> Probes { get; }
        
        /**************************************************************************/
        
        public ExecClassData(string testName, long? id, string className, int probeCnt)
        {
            Id = id;
            TestName = testName ?? throw new ArgumentNullException(nameof(testName));
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
            Probes = Enumerable.Repeat(false, probeCnt).ToList();
        }

        /**************************************************************************/

        public override string ToString()
        {
            return $"{TestName} -> {Id}: {ClassName}";
        }
    }
}

using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
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
        public List<bool> Probes { get; set; }
        
        /**************************************************************************/
        
        public ExecClassData(string testName, long? id, string className)
        {
            Id = id;
            TestName = testName ?? throw new ArgumentNullException(nameof(testName));
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
        }

        /**************************************************************************/

        public override string ToString()
        {
            return $"{TestName} -> {Id}: {ClassName}";
        }
    }
}

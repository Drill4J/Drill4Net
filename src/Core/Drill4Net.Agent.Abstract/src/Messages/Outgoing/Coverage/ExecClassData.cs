using System;
using System.Linq;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    //It's DTO: let properties will be with a small letter

    /// <summary>
    /// DTO: probes of instrumented assemblies for sending of collecting data to the Admin side
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record ExecClassData
    {
        /// <summary>
        /// Name of the executed test in target (user system)
        /// </summary>
        public string testName { get; }
        
        /// <summary>
        /// Can ignore
        /// </summary>
        public long? id { get; }

        /// <summary>
        /// Full class name
        /// </summary>
        public string className { get; }

        /// <summary>
        /// List of probe data
        /// </summary>
        public List<bool> probes { get; private set; }
        
        /**************************************************************************/
        
        public ExecClassData(string testName, string className)
        {
            id = className.GetHashCode();
            this.testName = testName ?? throw new ArgumentNullException(nameof(testName));
            this.className = className.Replace(".","/") ?? throw new ArgumentNullException(nameof(className));
        }

        /**************************************************************************/

        public void InitProbes(int probeCnt)
        {
            probes = Enumerable.Repeat(false, probeCnt).ToList();
        }

        public override string ToString()
        {
            return $"{testName} -> {className}";
        }
    }
}

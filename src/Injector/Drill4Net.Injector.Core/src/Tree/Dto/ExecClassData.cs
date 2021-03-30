using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class ExecClassData
    {
        public long? Id { get; set; }

        /// <summary>
        /// Full class name
        /// </summary>
        public string ClassName { get; set; }

        public List<bool> Probes { get; set; }
        public string TestName { get; set; }
    }
}

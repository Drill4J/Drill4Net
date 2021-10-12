using System;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    public class Test2Run
    {
        /// <summary>
        /// May be any?
        /// </summary>
        public string Name { get; set; }

        public long StartedAt { get; set; }
        public long FinishedAt { get; set; }
        public Test2RunInfo[] Tests { get; set; }
    }
}

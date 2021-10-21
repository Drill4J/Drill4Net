using System;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Risks methods info
    /// </summary>
    [Serializable]
    public record RiskCounter
    {
        /// <summary>
        /// Number of uncovered new methods
        /// </summary>
        public int New { get; set; }

        /// <summary>
        /// Number of uncovered modified methods
        /// </summary>
        public int Modified { get; set; }

        /// <summary>
        /// Number of risk methods in the build
        /// </summary>
        public int Total { get; set; }

        /*******************************************************************/

        public override string ToString()
        {
            return $"mod:{Modified}, new:{New}, total:{Total}";
        }
    }
}

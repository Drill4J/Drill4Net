using System;

namespace Drill4Net.Common
{
    /// <summary>
    /// Debug mode for the Tester
    /// </summary>
    /// <seealso cref="Drill4Net.Common.IDebugOptions" />
    [Serializable]
    public class TestetDebugOptions : IDebugOptions
    {
        /// <summary>
        /// Is Debug mode is active?
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }

        /// <summary>
        /// Whether to record the received samples in files (by methods).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [write probes]; otherwise, <c>false</c>.
        /// </value>
        public bool WriteProbes { get; set; }
    }
}

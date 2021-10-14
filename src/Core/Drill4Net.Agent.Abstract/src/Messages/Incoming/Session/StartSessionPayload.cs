using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    /// <summary>
    /// Metadata about started session
    /// </summary>
    [Serializable]
    public record StartSessionPayload
    {
        /// <summary>
        /// The session Uid
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// MANUAL or AUTO
        /// </summary>
        public string TestType { get; set; }

        public string TestName { get; set; }

        /// <summary>
        /// Is it a real-time session for immediate data updates in the Drill GUI?
        /// </summary>
        public bool IsRealtime { get; set; }

        /// <summary>
        /// Will the session be receive ALL probes from target?
        /// </summary>
        public bool IsGlobal { get; set; }
    }
}
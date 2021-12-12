namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Some parameters for the Standard Agent (instead of
    /// impossible parameters for any static constructor).
    /// </summary>
    public static class StandardAgentInitParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether skip the static constructor 
        /// of the Standard Agent and use the Init method instead.
        /// </summary>
        /// <value>
        ///   <c>true</c> if skip cctor; otherwise, <c>false</c>.
        /// </value>
        public static bool SkipCreatingSingleton { get; set; }

        /// <summary>
        /// Agent works in separate Worker (not in the Target's process directly)
        /// </summary>
        public static bool LocatedInWorker { get; set; }
    }
}

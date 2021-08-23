namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Some parameters for initializing the Standard Agent (instead of 
    /// impossible parameters for any static constructor).
    /// </summary>
    public static class StandardAgentCCtorParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether skip the static constructor 
        /// of the Standard Agent and use the Init method instead.
        /// </summary>
        /// <value>
        ///   <c>true</c> if skip cctor; otherwise, <c>false</c>.
        /// </value>
        public static bool SkipCctor { get; set; }
    }
}

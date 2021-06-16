namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the Debug mode
    /// </summary>
    public interface IDebugOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IDebugOptions"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }
    }
}

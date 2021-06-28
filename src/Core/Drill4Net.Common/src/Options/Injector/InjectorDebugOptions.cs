namespace Drill4Net.Common
{
    /// <summary>
    /// Debug category for the Injector options
    /// </summary>
    /// <seealso cref="Drill4Net.Common.IDebugOptions" />
    public class InjectorDebugOptions : IDebugOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IDebugOptions"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cross-point information is included into injected probe data.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cross-point information is included into injected probe data; otherwise, <c>false</c>.
        /// </value>
        public bool CrossPointInfo { get; set; }

        /// <summary>
        /// To continue on reading/writing/injection error on the processing assembly.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignore error; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreError { get; set; }

        /*******************************************************/

        public InjectorDebugOptions()
        {
            CrossPointInfo = true;
        }
    }
}

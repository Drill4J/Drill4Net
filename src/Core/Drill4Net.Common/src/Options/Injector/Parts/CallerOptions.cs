namespace Drill4Net.Common
{
    /// <summary>
    /// Base options for abstract Caller entity
    /// </summary>
    public class CallerOptions
    {
        /// <summary>
        /// Full class name
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Method's name
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Is method static?
        /// </summary>
        public bool Static { get; set; }
    }
}

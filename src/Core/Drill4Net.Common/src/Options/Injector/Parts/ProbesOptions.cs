namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the injecting process (types of methods, cross-points, etc)
    /// </summary>
    public class ProbesOptions
    {
        /// <summary>
        /// Need private methods?
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Need constructors?
        /// </summary>
        public bool Ctor { get; set; }

        /// <summary>
        /// Need getters of the properties?
        /// </summary>
        public bool Getter { get; set; }

        /// <summary>
        /// Need setters of the properties?
        /// </summary>
        public bool Setter { get; set; }

        /// <summary>
        /// Need handlers for the event adding?
        /// </summary>
        public bool EventAdd { get; set; }

        /// <summary>
        /// Need handlers for the event removing?
        /// </summary>
        public bool EventRemove { get; set; }

        /// <summary>
        /// Does the Enter type of cross-point needed?
        /// </summary>
        public bool SkipEnterType { get; set; }

        /// <summary>
        /// Does the IfElse type of cross-point needed?
        /// </summary>
        public bool SkipIfElseType { get; set; }

        /************************************************************/

        /// <summary>
        /// Create options for the injecting process (types of methods, cross-points, etc)
        /// </summary>
        public ProbesOptions()
        {
            Private = true;
            Ctor = true;
            Getter = true;
            Setter = true;
        }
    }
}

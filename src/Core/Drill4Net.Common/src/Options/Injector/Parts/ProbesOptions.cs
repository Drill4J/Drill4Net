namespace Drill4Net.Common
{
    public class ProbesOptions
    {
        public bool Private { get; set; }
        public bool Ctor { get; set; }
        public bool Getter { get; set; }
        public bool Setter { get; set; }
        public bool EventAdd { get; set; }
        public bool EventRemove { get; set; }

        /// <summary>
        /// Does the Enter type of cross-point needed?
        /// </summary>
        public bool SkipEnterType { get; set; }

        /***********************************************/

        public ProbesOptions()
        {
            Private = true;
            Ctor = true;
            Getter = true;
            Setter = true;
        }

    }
}

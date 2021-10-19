using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public abstract record AbstractMessage
    {
        /// <summary>
        /// Type of the messge (in fact, the its topic)
        /// </summary>
        public string type { get; set; }

        /***********************************************************/

        protected AbstractMessage() { }

        protected AbstractMessage(string type)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /***********************************************************/

        public override string ToString()
        {
            return type;
        }
    }
}
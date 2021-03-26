using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class AstMethod
    {
        /// <summary>
        /// Namespace? Full source?
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Name of the method.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Lists the parameters.
        /// </summary>
        public List<string> Params { get; set; }

        /// <summary>
        /// The return type.
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// The identifiers of the probes added to the method.
        /// </summary>
        public List<bool> Probes { get; set; }

        /// <summary>
        /// The count of probes added to the method.
        /// </summary>
        public int Count { get; set; } //don't used?

        public string Checksum { get; set; }
    }
}

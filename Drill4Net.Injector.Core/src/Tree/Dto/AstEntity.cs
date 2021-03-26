using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Data about a class sent to Drill admin.
    /// </summary>
    public class AstEntity
    {
        /// <summary>
        /// The assembly where the class is located.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The name of the class, with namespace.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Information about methods in the class.
        /// </summary>
        public List<AstMethod> Methods { get; set; }
    }
}

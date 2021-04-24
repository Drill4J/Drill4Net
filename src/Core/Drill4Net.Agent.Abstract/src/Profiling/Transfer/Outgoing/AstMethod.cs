using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    /// <summary>
    /// Class for presentation of method metadata for transferring to the Drill site
    /// </summary>
    public class AstMethod
    {
        /// <summary>
        /// Namespace? Full source?
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Name of the method.
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// Lists the parameters.
        /// </summary>
        public List<string> Params { get; set; }

        /// <summary>
        /// The return type.
        /// </summary>
        public string ReturnType { get; }

        /// <summary>
        /// The identifiers of the probes added to the method.
        /// </summary>
        public List<bool> Probes { get; }

        /// <summary>
        /// The count of probes added to the method.
        /// </summary>
        public int Count { get; } //don't used?

        public string Checksum { get; }
        
        /******************************************************************************/
        public AstMethod() { }

        public AstMethod(string path, string name, string returnType, int probeCount, string checksum)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            Count = probeCount;
            Checksum = checksum ?? throw new ArgumentNullException(nameof(checksum));
            Params = new List<string>();
            Probes = new List<bool>();
        }
        
        /******************************************************************************/
        
        public override string ToString()
        {
            return Name;
        }
    }
}

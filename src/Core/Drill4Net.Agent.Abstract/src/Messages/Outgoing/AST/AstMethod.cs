using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    /// <summary>
    /// Class for presentation of method metadata for transferring to the Drill site
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class AstMethod
    {
        /// <summary>
        /// Name of the method.
        /// </summary>
        public string name { get;  }

        /// <summary>
        /// Lists the parameters.
        /// </summary>
        public List<string> @params { get; set; }

        /// <summary>
        /// The return type.
        /// </summary>
        public string returnType { get; }

        /// <summary>
        /// The identifiers of the probes added to the method.
        /// </summary>
        public List<bool> probes { get; }

        /// <summary>
        /// The count of probes added to the method.
        /// </summary>
        public int count { get; } //don't used?

        public string checksum { get; }
        
        /******************************************************************************/
        public AstMethod() { }

        public AstMethod(string name, string returnType, int probeCount, string checksum)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.returnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            count = probeCount;
            this.checksum = checksum ?? throw new ArgumentNullException(nameof(checksum));
            @params = new List<string>();
            probes = new List<bool>();
        }
        
        /******************************************************************************/
        
        public override string ToString()
        {
            return name;
        }
    }
}

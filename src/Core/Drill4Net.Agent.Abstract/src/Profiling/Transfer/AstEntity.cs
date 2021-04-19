using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Data about a class sent to Drill admin.
    /// </summary>
    public class AstEntity
    {
        /// <summary>
        /// The assembly where the class is located.
        /// </summary>
        public string Path { get;  }

        /// <summary>
        /// The fullName of the class, with namespace.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Information about methods in the class.
        /// </summary>
        public List<AstMethod> Methods { get; }
        
        /**************************************************************************/
        
        public AstEntity(string path, string fullName)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Name = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Methods = new List<AstMethod>();
        }
        
        /**************************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}

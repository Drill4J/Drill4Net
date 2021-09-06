using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    //It's DTO: let properties will be with a small letter

    /// <summary>
    /// DTO: data about a class sent to Drill admin.
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record AstEntity
    {
        /// <summary>
        /// The assembly where the class is located.
        /// </summary>
        public string path { get;  }

        /// <summary>
        /// The fullName of the class, with namespace.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// Information about methods in the class.
        /// </summary>
        public List<AstMethod> methods { get; }
        
        /**************************************************************************/
        
        public AstEntity(string entityPath, string fullName)
        {
            path = entityPath.Replace(".", "/") ?? throw new ArgumentNullException(nameof(entityPath));
            name = fullName ?? throw new ArgumentNullException(nameof(fullName));
            methods = new List<AstMethod>();
        }
        
        /**************************************************************************/

        public override string ToString()
        {
            return $"{path}.{name}";
        }
    }
}

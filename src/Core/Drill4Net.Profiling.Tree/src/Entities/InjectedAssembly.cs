using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Common;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedAssembly : InjectedEntity
    {
        public AssemblyVersioning Version { get; }

        /*********************************************************************/

        public InjectedAssembly(AssemblyVersioning version, string name, string fullName, string path) : base(name, name, path)
        {
            FullName = fullName;
            Version = version;
        }

        /*********************************************************************/

        public IEnumerable<InjectedType> GetAllTypes()
        {
            return Flatten(typeof(InjectedMethod))
                .Where(a => a.GetType().Name == nameof(InjectedType))
                .Cast<InjectedType>();
        }

        public IEnumerable<InjectedType> GetTypes()
        {
            return _children.Where(a => a.GetType().Name == nameof(InjectedType))
                 .Cast<InjectedType>();
        }

        public override string ToString()
        {
            return $"{base.ToString()}{Name}";
        }
    }
}

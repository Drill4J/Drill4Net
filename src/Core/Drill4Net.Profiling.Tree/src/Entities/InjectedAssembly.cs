using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedAssembly : InjectedEntity
    {
        public bool IsProcessed { get; set; }
        public AssemblyVersioning FrameworkVersion { get; }
        public string ProductVersion { get; }

        /*********************************************************************/

        public InjectedAssembly(AssemblyVersioning version, string name, string fullName, string path): base(name, name, path)
        {
            FullName = fullName;
            FrameworkVersion = version;
            ProductVersion = FileUtils.GetProductVersion(path);
        }

        /*********************************************************************/

        public IEnumerable<InjectedType> GetAllTypes()
        {
            return Flatten(typeof(InjectedMethod))
                .Where(a => a.GetType().Name == nameof(InjectedType))
                .Cast<InjectedType>();
        }

        public IEnumerable<InjectedMethod> GetAllMethods()
        {
            return Flatten(typeof(CrossPoint))
                .Where(a => a.GetType().Name == nameof(InjectedMethod))
                .Cast<InjectedMethod>();
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

using System;
using Drill4Net.Common;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedAssembly : InjectedEntity
    {
        public AssemblyVersioning Version { get; set; }

        /*********************************************************************/

        public InjectedAssembly(string name, string fullName, string path) : base(name, name, path)
        {
            FullName = fullName;
        }

        /*********************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}

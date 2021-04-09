using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedAssembly : InjectedEntity
    {
        public AssemblyVersion Version { get; set; }

        /*********************************************************************/

        public InjectedAssembly(string name, string fullName, string path) : base(name, name, path)
        {
            Fullname = fullName;
        }

        /*********************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}

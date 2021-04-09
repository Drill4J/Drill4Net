using System;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedEntity : InjectedSimpleEntity
    {
        public string Fullname { get; set; }
        public string AssemblyName { get; set; }

        /****************************************************************/

        public InjectedEntity()
        {
        }
        
        public InjectedEntity(string name) : base(name)
        {
        }

        public InjectedEntity(string assemblyName, string name, string path) : base(name, path)
        {
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
        }

        /****************************************************************/

        public InjectedSimpleEntity GetByFullname(string fullname)
        {
            return _children.Cast<InjectedEntity>().FirstOrDefault(a => a.Fullname == fullname);
        }
    }
}

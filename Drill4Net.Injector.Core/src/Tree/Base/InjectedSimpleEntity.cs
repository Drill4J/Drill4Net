using System;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedSimpleEntity : GenericTree<InjectedSimpleEntity>
    {
        public string Name { get; set; } 
        public string Path { get; set; }

        /*********************************************************************/

        public InjectedSimpleEntity()
        {
        }

        public InjectedSimpleEntity(string name, string path = null)
        {
            Name = name;
            Path = path;
        }

        /*********************************************************************/

        public InjectedSimpleEntity GetByName(string name)
        {
            return _children.FirstOrDefault(a => a.Name == name);
        }
    }
}

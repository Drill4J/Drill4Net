using System;
using System.Linq;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
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

        internal void CleanEntities(List<InjectedSimpleEntity> list, Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> parents)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var ent = list[i];
                if (ent.Count > 0)
                    continue;
                var parent = parents[ent];
                parent.Remove(ent);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

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

        public Dictionary<string, InjectedSimpleEntity> CalcPointMap(Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> parentMap)
        {
            var pointMap = new Dictionary<string, InjectedSimpleEntity>();
            var pointPairs = parentMap.Where(a => a.Key is CrossPoint);
            foreach (var pointPair in pointPairs)
            {
                var point = (CrossPoint)pointPair.Key;
                pointMap.Add((point).PointUid, point);
            }
            return pointMap;
        }
    }
}

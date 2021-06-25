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

        /// <summary>
        /// Get parent map of <see cref="CrossPoint"/> by it's <see cref="CrossPoint.PointUid"/>. 
        /// </summary>
        /// <param name="parentMap">Parent hiearchy map of entities. If empty it will calc</param>
        /// <returns></returns>
        public Dictionary<string, CrossPoint> MapPoints(Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> parentMap = null)
        {
            var pointMap = new Dictionary<string, CrossPoint>();
            if (parentMap == null)
                parentMap = CalcParentMap();
            var pointPairs = parentMap.Where(a => a.Key is CrossPoint);
            foreach (var pointPair in pointPairs)
            {
                var point = (CrossPoint)pointPair.Key;
                var uid = point.PointUid;
                if(!pointMap.ContainsKey(uid))
                    pointMap.Add(uid, point);
            }
            return pointMap;
        }

        /// <summary>
        /// Get parent map of <see cref="InjectedMethod"/> by corresponding <see cref="CrossPoint.PointUid"/>. 
        /// </summary>
        /// <param name="parentMap">The hierarchy map of entity parents. If empty it will calc</param>
        /// <returns></returns>
        public Dictionary<string, InjectedMethod> MapPointToMethods(Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> parentMap = null)
        {
            var methodMap = new Dictionary<string, InjectedMethod>();
            if (parentMap == null)
                parentMap = CalcParentMap();
            var pointPairs = parentMap.Where(a => a.Key is CrossPoint);
            foreach (var pointPair in pointPairs)
            {
                var point = (CrossPoint)pointPair.Key;
                var uid = point.PointUid;
                if (parentMap[point] is not InjectedMethod method)
                    continue;
                if (!methodMap.ContainsKey(uid))
                    methodMap.Add(uid, method);
            }
            return methodMap;
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

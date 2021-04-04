using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class GenericTree<T> where T : GenericTree<T>
    {
        //public GenericTree<T> Parent { get; set; }

        protected List<T> _children;

        /*******************************************************************/

        public GenericTree()
        {
            _children = new List<T>();
        }

        /*******************************************************************/

        public virtual void AddChild(T newChild)
        {
            if (newChild == null)
                throw new ArgumentNullException(nameof(newChild));
            //
            //newChild.Parent = this;
            _children.Add(newChild);
        }

        public void AddChild(List<T> range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));
            foreach (var p in range)
                AddChild(p);
        }

        public bool Remove(T node)
        {
            return _children.Remove(node);
        }

        public IEnumerable<T> Flatten(Type breakOn = null)
        {
            if (breakOn != null && GetType().Name == breakOn.Name)
                return Enumerable.Empty<T>();
            var filter = _children.Where(a => breakOn == null || (breakOn != null && a.GetType().Name != breakOn.Name));
            return _children.SelectMany(x => x.Flatten(breakOn)).Concat(filter);
        }

        public IEnumerable<T> Filter(Type flt, bool inDepth)
        {
            var filter = _children.Where(a => flt == null || (flt != null && a.GetType() == flt));
            if (!inDepth)
                return filter;
            return _children.SelectMany(x => x.Filter(flt, inDepth)).Concat(filter);
        }

        public void Traverse(Action<int, T, T> visitor)
        {
            Traverse(0, visitor, null);
        }

        protected virtual void Traverse(int depth, Action<int, T, T> visitor, T parent)
        {
            var @this = (T)this;
            visitor(depth, @this, parent);
            foreach (T child in _children)
                child.Traverse(depth + 1, visitor, @this);
        }

        public Dictionary<T, T> CalcParentMap()
        {
            var map = new Dictionary<T, T>();
            Traverse((int level, T child, T parent) => map.Add(child, parent));
            return map;
        }
    }
}

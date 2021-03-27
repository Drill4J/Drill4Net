using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Injector.Core
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
            return _children.SelectMany(x => x.Flatten(breakOn)).Concat(_children);
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

        public Dictionary<T, T> CalcMap()
        {
            var map = new Dictionary<T, T>();
            Traverse((int level, T child, T parent) => map.Add(child, parent));
            return map;
        }
    }
}

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

        public void Traverse(Action<int, T> visitor)
        {
            Traverse(0, visitor);
        }

        protected virtual void Traverse(int depth, Action<int, T> visitor)
        {
            visitor(depth, (T)this);
            foreach (T child in _children)
                child.Traverse(depth + 1, visitor);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class GenericTree<T> where T : GenericTree<T>, new()
    {
        public Guid Uid { get; }

        public bool IsParentHub { get; set; }
        public bool? IsShared { get; set; }

        public T this[int index]
        {
            get
            {
                return _children[index];
            }
            set
            {
                _children[index] = value;
            }
        }

        public int Count => _children.Count;

        protected List<T> _children;

        /*******************************************************************/

        public GenericTree()
        {
            Uid = Guid.NewGuid();
            _children = new List<T>();
        }

        /*******************************************************************/

        public virtual void Add(T newChild)
        {
            if (newChild == null)
                throw new ArgumentNullException(nameof(newChild));
            _children.Add(newChild);
        }

        public void Add(List<T> range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));
            foreach (var p in range)
                Add(p);
        }

        public bool Remove(T node)
        {
            return _children.Remove(node);
        }

        public bool Contains(T child, bool inDeep = false, Type breakOn = null)
        {
            if (!inDeep)
                return _children.Contains(child);
            var all = Flatten(breakOn);
            return all.Contains(child);
        }

        public IEnumerable<T> Flatten(Type breakOn = null)
        {
            if (breakOn != null && GetType().Name == breakOn.Name)
                return Enumerable.Empty<T>();
            var filter = _children.Where(a => breakOn == null || (a.GetType().Name != breakOn.Name));
            return _children.SelectMany(x => x.Flatten(breakOn)).Concat(filter);
        }

        /// <summary>
        /// Filter children's entities of current entity (as a parent)
        /// </summary>
        /// <param name="flt"></param>
        /// <param name="inDepth"></param>
        /// <returns></returns>
        public IEnumerable<T> Filter(Type flt, bool inDepth)
        {
            var filter = _children.Where(a => flt == null || a.GetType() == flt);
            if (!inDepth)
                return filter;
            return _children.SelectMany(x => x.Filter(flt, true)).Concat(filter);
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

        public virtual Dictionary<T, T> CalcParentMap()
        {
            var map = new Dictionary<T, T>();
            Traverse((_, child, parent) =>
            {
                if (!map.ContainsKey(child))
                {
                    map.Add(child, parent); //child:parent = 1:1
                }
                else
                {
                    var prev = map[child];
                    if (prev == parent || prev.Contains(parent) || parent.Contains(prev))
                        return;

                    // more than 1 parents for 1 child
                    child.IsShared = true;
                    T parentHub = null;
                    if (prev.IsParentHub)
                    {
                        parentHub = prev;
                    }
                    else
                    {
                        parentHub = new T() { IsParentHub = true };
                        parentHub.Add(prev);
                    }
                    parentHub.Add(parent);
                    map[child] = parentHub;
                }
            });
            return map;
        }

        public override string ToString()
        {
            if (IsShared == true) return "*";
            if (IsParentHub) return "#";
            return null;
        }
    }
}

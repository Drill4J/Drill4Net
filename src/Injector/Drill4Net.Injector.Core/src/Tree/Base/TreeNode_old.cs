using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    public class TreeNode_old<T> where T : class
    {
        #region Indexator
        private readonly List<TreeNode_old<T>> _children;
        public TreeNode_old<T> this[int index]
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
        #endregion

        public ReadOnlyCollection<TreeNode_old<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public Guid Uid { get; set; }
        public bool IsRoot => Parent == null;
        public TreeNode_old<T> Parent { get; protected set; }
        public T Value { get; set; }

        /************************************************************/

        public TreeNode_old(T data)
        {
            Uid = Guid.NewGuid();
            _children = new List<TreeNode_old<T>>();
            Value = data;
        }

        /************************************************************/

        public TreeNode_old<T> Add(T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            //
            var node = new TreeNode_old<T>(data)
            {
                Parent = this
            };
            _children.Add(node);
            return node;
        }

        public TreeNode_old<T>[] Add(params T[] values)
        {
            return values.Select(Add).ToArray();
        }

        public void Add(List<T> range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));
            foreach (var p in range)
                Add(p);
        }

        public bool Remove(TreeNode_old<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(_children.SelectMany(x => x.Flatten()));
        }
    }
}

using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    public class InjectedEntityComparer<T> : EqualityComparer<T> where T : InjectedEntity
    {
        public override bool Equals(T x, T y)
        {
             return x?.FullName != null && y?.FullName != null &&
                x.FullName.Equals(y.FullName, System.StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode(T obj)
        {
            if (obj?.FullName == null)
                return -1;
            return obj.FullName.GetHashCode();
        }
    }
}

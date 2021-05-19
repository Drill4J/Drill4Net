using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    public class InjectedMethodComparer : EqualityComparer<InjectedMethod>
    {
        public override bool Equals(InjectedMethod x, InjectedMethod y)
        {
             return x?.FullName != null && y?.FullName != null && x.FullName.Equals(y.FullName, System.StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode(InjectedMethod obj)
        {
            if (obj?.FullName == null)
                return -1;
            return obj.FullName.GetHashCode();
        }
    }
}

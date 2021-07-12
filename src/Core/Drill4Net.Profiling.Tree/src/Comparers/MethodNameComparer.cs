using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Comparer for the <see cref="InjectedMethod"/> by its short name + parameter list
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IComparer{Drill4Net.Profiling.Tree.InjectedMethod}" />
    public class MethodNameComparer : IComparer<InjectedMethod>
    {
        public int Compare(InjectedMethod x, InjectedMethod y)
        {
            //better to compare by fullname (with parameter list) but without the leading return type
            //(not just by short name) to take into account the overloads of method for unambiguity
            var xAr = x?.FullName?.Split(' ');
            if (xAr == null || xAr.Length < 2)
                return -1;
            var xname = xAr[1];
            //
            var yAr = y?.FullName?.Split(' ');
            if (yAr == null || yAr.Length < 2)
                return 1;
            var yname = yAr[1];
            //
            return string.Compare(xname, yname, true);
        }
    }
}

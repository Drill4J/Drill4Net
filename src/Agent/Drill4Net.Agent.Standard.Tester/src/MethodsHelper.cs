using System.Linq;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;



namespace Drill4Net.Agent.Standard.Tester
{
    internal static class MethodsHelper
    {
        internal static List<InjectedMethod> GetSortedMethods(Dictionary<string, InjectedMethod> methods)
        {
            return methods.Values
                .OrderBy(a => a.AssemblyName)
                .ThenBy(a => a.BusinessType)
                .ThenBy(a => a.Name) //more presentable than through FullName (due Return type, namespaces, etc it won't be alphabetical strict)
                .ToList();
        }

        internal static Dictionary<int, InjectedMethod> GetMethodByOrderNumber(List<InjectedMethod> sorted)
        {
            var res = new Dictionary<int, InjectedMethod>();
            for (var i = 0; i < sorted.Count; i++)
                res.Add(i + 1, sorted[i]);
            return res;
        }
    }
}

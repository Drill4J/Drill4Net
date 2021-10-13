using Drill4Net.Profiling.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Standard.Tester
{
    public class TreeInfo
    {
        public InjectedSolution injSolution;
        public InjectedDirectory injDirectory;
        public string targetPath;
        public Dictionary<string, InjectedMethod> methods;
        public TesterOptions opts;
        public List<InjectedMethod> methodSorted;
        public Dictionary<int, InjectedMethod> methodByOrderNumber;
        public List<CrossPoint> points;
    }
}

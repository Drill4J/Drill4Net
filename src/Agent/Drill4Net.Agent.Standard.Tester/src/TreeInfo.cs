using Drill4Net.Profiling.Tree;
using System.Collections.Generic;

namespace Drill4Net.Agent.Standard.Tester
{
    public class TreeInfo
    {
        public InjectedSolution InjSolution { get; set; }
        public InjectedDirectory InjDirectory { get; set; }
        public string TargetPath { get; set; }
        public Dictionary<string, InjectedMethod> Methods { get; set; }
        public TesterOptions Opts { get; set; }
        public List<InjectedMethod> MethodSorted { get; set; }
        public Dictionary<int, InjectedMethod> MethodByOrderNumber { get; set; }
        public List<CrossPoint> Points { get; set; }
    }
}

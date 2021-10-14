using Drill4Net.Profiling.Tree;
using System.Collections.Generic;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Tree Info for the Tester app
    /// </summary>
    public class TesterTreeInfo
    {
        /// <summary>
        /// The metadata of the injected Target projects
        /// </summary>
        public InjectedSolution InjSolution { get; set; }

        /// <summary>
        /// The TesterOptions tree folder  directory of injected projects
        /// </summary>
        public InjectedDirectory InjDirectory { get; set; }

        /// <summary>
        /// Contains current directory from TesterOptions
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Injected methods
        /// </summary>
        public Dictionary<string, InjectedMethod> Methods { get; set; }

        /// <summary>
        /// Options for the Tester app
        /// </summary>
        public TesterOptions Opts { get; set; }

        /// <summary>
        /// Sorted injected methods
        /// </summary>
        public List<InjectedMethod> MethodSorted { get; set; }

        /// <summary>
        /// Injected methods ordered by number
        /// </summary>
        public Dictionary<int, InjectedMethod> MethodByOrderNumber { get; set; }

        /// <summary>
        /// The list of cross-cutting points
        /// </summary>
        public List<CrossPoint> Points { get; set; }
    }
}

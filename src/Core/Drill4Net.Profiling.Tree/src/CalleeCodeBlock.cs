using System;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CalleeCodeBlock
    {
        public Dictionary<InjectedMethod, int> CallerIndexes { get; }
        public int FirstIndex { get; set; } = -1;
        public int LastIndex { get; set; } = -1;
        
        public CalleeCodeBlock()
        {
            CallerIndexes = new Dictionary<InjectedMethod, int>();
        }

    }
}
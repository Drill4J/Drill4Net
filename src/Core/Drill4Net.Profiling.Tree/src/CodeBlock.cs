using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CodeBlock
    {
        public int FirstIndex { get; set; } = -1;
        public int LastIndex { get; set; } = -1;
    }
}
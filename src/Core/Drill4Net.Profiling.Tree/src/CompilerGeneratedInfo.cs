using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CompilerGeneratedInfo
    {
        public int CallIndex { get; set; } = -1;
        public int FirstIndex { get; set; } = -1;
        public int LastIndex { get; set; } = -1;
    }
}
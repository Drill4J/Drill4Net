using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class TypeSource : BaseSource
    {
        public string FilePath { get; set; }
        public bool IsValueType { get; set; }
     }
}

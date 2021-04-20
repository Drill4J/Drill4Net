using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class TypeSource : BaseSource
    {
        public bool IsValueType { get; set; }
     }
}

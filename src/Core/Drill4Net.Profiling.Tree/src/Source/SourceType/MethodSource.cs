using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class MethodSource : BaseSource
    {
        public MethodType MethodType { get; set; }
        public string HashCode { get; set; }
        public bool IsOverride { get; set; } //?
        
        public bool IsMoveNext { get; set; }
        public bool IsEnumeratorMoveNext { get; set; }
        public bool IsFinalizer { get; set; }

    }
}

using System;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Metadata about method
    /// </summary>
    [Serializable]
    public class MethodSource : BaseSource
    {
        /// <summary>
        /// Type of method
        /// </summary>
        public MethodType MethodType { get; set; }

        /// <summary>
        /// Hash code of method body for the tracking of it's changes
        /// </summary>
        public string HashCode { get; set; }

        public bool IsOverride { get; set; } //?

        public bool IsAsyncStateMachine { get; set; }
        public bool IsMoveNext { get; set; }
        public bool IsEnumeratorMoveNext { get; set; }
        public bool IsFinalizer { get; set; }
    }
}

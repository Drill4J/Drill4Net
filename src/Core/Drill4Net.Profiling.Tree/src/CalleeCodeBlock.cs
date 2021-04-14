using System;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Info for compiler generated methods (caller, indexes, etc)
    /// </summary>
    [Serializable]
    public class CalleeCodeBlock
    {
        /// <summary>
        /// Caller of current (compiler generated) method
        /// </summary>
        public InjectedMethod Caller { get; set;}
        
        /// <summary>
        /// The instruction index of the caller's IL code where this callee is called (in fact, parent index) 
        /// </summary>
        public int CallerIndex { get; set;}
        
        /// <summary>
        /// The first index in the compiler generated code which considered as 'business part'
        /// </summary>
        public int FirstIndex { get; set; } = -1;
        
        /// <summary>
        /// The last index in the compiler generated code which considered as 'business part'
        /// </summary>
        public int LastIndex { get; set; } = -1;
    }
}
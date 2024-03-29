﻿using System;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Info for compiler generated methods (caller, indexes, etc)
    /// </summary>
    [Serializable]
    public class CompilerGeneratedInfo
    {
        public string FromMethod { get; set; }
        
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
    }
}
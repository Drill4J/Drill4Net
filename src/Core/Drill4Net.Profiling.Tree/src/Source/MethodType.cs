﻿namespace Drill4Net.Profiling.Tree
{
    public enum MethodType
    {
        /// <summary>
        /// Type not specified
        /// </summary>
        Unset = 0,
        
        /// <summary>
        /// Such methods are generated by the compiler (usually in special classes)
        /// and are logically part of the code of other business methods
        /// </summary>
        CompilerGeneratedPart = 1,
        
        /// <summary>
        /// Normal ('business') method from the user source code
        /// </summary>
        Normal = 2,
        
        Constructor = 3,
        Destructor = 4,
        //Anonymous = 5,
        Local = 8,
        Getter = 9,
        Setter = 10,
        EventAdd = 11,
        EventRemove = 12,
    }
}
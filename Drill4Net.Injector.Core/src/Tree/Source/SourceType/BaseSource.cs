using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class BaseSource
    {
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsGeneric { get; set; } //?
        public bool IsNested { get; set; }
        //public bool IsAnonymous { get; set; } //?
        public AccessType AccessType { get; set; }
    }
}

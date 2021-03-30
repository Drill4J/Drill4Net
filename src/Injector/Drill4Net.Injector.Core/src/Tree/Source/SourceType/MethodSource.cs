using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class MethodSource : BaseSource
    {
        public MethodType MethodType { get; set; }
        public string HashCode { get; set; }
        public bool IsOverride { get; set; } //?
    }
}

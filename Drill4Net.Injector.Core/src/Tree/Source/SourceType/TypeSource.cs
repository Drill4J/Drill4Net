using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class TypeSource : BaseSource
    {
        public bool IsValueType { get; set; }
    }
}

using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class ClassSource : BaseSource
    {
        public bool IsValueType { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedMethod : InjectedEntity
    {
        public List<CrossPoint> Points { get; set; }

        public MethodSource Source { get; set; }

        /********************************************************************/

        public InjectedMethod(string nameSpace, string name) : base(name)
        {
            Fullname = string.IsNullOrWhiteSpace(nameSpace) ? name : $"{nameSpace}.{name}";
            Points = new List<CrossPoint>();
        }

        /********************************************************************/

        public override string ToString()
        {
            return Fullname;
        }
    }
}

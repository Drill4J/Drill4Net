using System;
using Drill4Net.Injector.Core;

namespace Drill4Net.Target.Comon.Tests
{
    internal class PointLinkage : IComparable
    {
        public InjectedAssembly Assembly { get; }
        public InjectedClass Type { get; }
        public InjectedMethod Method { get; }
        public CrossPoint Point { get; }
        public string Probe { get; }

        /****************************************************************************/

        public PointLinkage(InjectedAssembly assembly, InjectedClass type, 
            InjectedMethod method,  CrossPoint point)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Point = point ?? throw new ArgumentNullException(nameof(point));
            Probe = $"{point.PointType}_{point.PointId}";
        }

        /****************************************************************************/

        public override string ToString()
        {
            return Point.ToString();
        }

        public int CompareTo(object obj)
        {
            return Probe.CompareTo((obj as PointLinkage)?.Probe);
        }
    }
}

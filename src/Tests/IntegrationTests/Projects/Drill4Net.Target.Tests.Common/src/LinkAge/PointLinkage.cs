using System;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Target.Tests.Common
{
    /// <summary>
    /// Info linking cross-point with its method, type, assembly, etc
    /// </summary>
    /// <seealso cref="System.IComparable" />
    internal class PointLinkage : IComparable
    {
        public InjectedAssembly Assembly { get; }
        public InjectedType Type { get; }
        public InjectedMethod Method { get; }
        public CrossPoint Point { get; }

        /// <summary>
        /// Gets the cross-point probe's data.
        /// </summary>
        /// <value>
        /// The probe's data.
        /// </value>
        public string Probe { get; }

        /****************************************************************************/

        public PointLinkage(InjectedAssembly assembly, InjectedType type, 
            InjectedMethod method,  CrossPoint point)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Point = point ?? throw new ArgumentNullException(nameof(point));
            Probe = $"{point.PointType}_{point.PointId}";
        }

        /****************************************************************************/

        public int CompareTo(object obj)
        {
            return Probe.CompareTo((obj as PointLinkage)?.Probe);
        }

        public override string ToString()
        {
            return Point.ToString();
        }
    }
}

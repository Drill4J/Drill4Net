using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Strategies.Flow
{
    /// <summary>
    /// Helper for the generating the cross-point's probe data
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.AbstractProbeHelper" />
    public class FlowProbeHelper : AbstractProbeHelper
    {
        private readonly InjectorDebugOptions _dbgOpts;

        /*********************************************************************************/

        public FlowProbeHelper(InjectorDebugOptions dbgOpts = null)
        {
            _dbgOpts = dbgOpts;
        }

        /*********************************************************************************/

        protected override string GenerateProbePrefix(MethodContext ctx, CrossPoint point)
        {
            if (_dbgOpts?.CrossPointInfo == true)
            {
                var injMeth = ctx.Method;
                return $"{point.PointUid}^{injMeth.AssemblyName}^{injMeth.BusinessMethod}^";
            }
            else
            {
                return point.PointUid;
            }
        }

        protected override string GenerateProbeData(CrossPoint point)
        {
            if (_dbgOpts?.CrossPointInfo == true)
            {
                var str = point.PointType;
                return point.PointId == point.BusinessIndex.ToString()
                    ? $"{str}_{point.BusinessIndex}"
                    : $"{str}_{point.PointId}/{point.BusinessIndex}";
            }
            else
            {
                return null;
            }
        }
    }
}
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

        /// <summary>
        /// Generates the main part of cross-point's probe data.
        /// </summary>
        /// <param name="ctx">The method's context.</param>
        /// <param name="point">The cross-point of the target code.</param>
        protected override string GenerateProbeData(MethodContext ctx, CrossPoint point)
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

        /// <summary>
        /// Generates the specific probe data - its type, id, etc (as a rule for the debugging).
        /// </summary>
        /// <param name="point">The cross-point of the target code.</param>
        /// <returns></returns>
        protected override string GenerateSpecificProbeData(CrossPoint point)
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
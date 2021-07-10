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
        public FlowProbeHelper(InjectorOptions opts): base(opts)
        {
        }

        /*********************************************************************************/

        /// <summary>
        /// Generates the main part of cross-point's probe data.
        /// </summary>
        /// <param name="ctx">The method's context.</param>
        /// <param name="point">The cross-point of the target code.</param>
        protected override string GenerateProbeData(MethodContext ctx, CrossPoint point)
        {
            if (Options?.Debug?.CrossPointInfo == true)
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
            if (Options?.Debug?.CrossPointInfo == true)
            {
                var str = point.PointType;
                return point.OrigInd == point.BusinessIndex || point.BusinessIndex == -1
                    ? $"{str}_{point.OrigInd}"
                    : $"{str}_{point.OrigInd}/{point.BusinessIndex}";
            }
            else
            {
                return null;
            }
        }
    }
}
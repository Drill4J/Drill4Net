using System;
using System.Linq;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Base helper for the handling different types of cross-points and theirs probe's data
    /// </summary>
    public abstract class AbstractProbeHelper
    {
        public InjectorOptions Options { get; }

        /*********************************************************************************/

        protected AbstractProbeHelper(InjectorOptions options)
        {
            Options = options;
        }

        /*********************************************************************************/

        #region GenerateProbe        
        /// <summary>
        /// Generates the cross-point's probe data in the string format 
        /// for the injecting into Target's method.
        /// </summary>
        /// <param name="ctx">The target method's context.</param>
        /// <param name="point">The cross-point of the target code.</param>
        /// <returns></returns>
        public virtual string GenerateProbe(MethodContext ctx, CrossPoint point)
        {
            return $"{GenerateProbeData(ctx, point)}{GenerateSpecificProbeData(point)}";
        }

        /// <summary>
        /// Generates the main part of cross-point's probe data.
        /// </summary>
        /// <param name="ctx">The method's context.</param>
        /// <param name="point">The cross-point of the target code.</param>
        /// <returns></returns>
        protected abstract string GenerateProbeData(MethodContext ctx, CrossPoint point);

        /// <summary>
        /// Generates the specific probe data - its type, id, etc (as a rule for the debugging).
        /// </summary>
        /// <param name="point">The cross-point of the target code.</param>
        /// <returns></returns>
        protected abstract string GenerateSpecificProbeData(CrossPoint point);
        #endregion
        #region CrossPoint        
        /// <summary>
        /// Gets the cross-point of the Target's code by its type and localId (create if needed).
        /// </summary>
        /// <param name="ctx">The target method's context</param>
        /// <param name="pointType">Type of the cross-point.</param>
        /// <param name="origInd">The local identifier of the cross-point.</param>
        /// <returns></returns>
        public virtual CrossPoint GetOrCreatePoint(MethodContext ctx, CrossPointType pointType, int origInd)
        {
            var point = ctx.Method.Points
                .SingleOrDefault(a => a.PointType == pointType && a.OrigInd == origInd); //check for PointType need to use also
            if (point != null)
                return point;
            point = CreateCrossPoint(ctx, pointType, origInd);
            ctx.Method.Add(point);
            return point;
        }

        /// <summary>
        /// Creates the cross-point of the Target's code by its type and localId.
        /// </summary>
        /// <param name="ctx">The target method's context</param>
        /// <param name="pointType">Type of the cross-point.</param>
        /// <param name="origInd">The local identifier of the cross-point.</param>
        /// <returns></returns>
        protected virtual CrossPoint CreateCrossPoint(MethodContext ctx, CrossPointType pointType, int origInd)
        {
            var pointUid = Guid.NewGuid().ToString();
            var bizInd = ctx.GetLocalBusinessIndex(origInd);
            var point = new CrossPoint(pointUid, origInd, bizInd, pointType)
            {
                //TODO: bind PDB data
            };
            return point;
        }
        #endregion
    }
}

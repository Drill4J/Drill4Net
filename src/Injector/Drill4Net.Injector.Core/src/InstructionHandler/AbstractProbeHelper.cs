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
            return $"{GenerateProbePrefix(ctx, point)}{GenerateProbeData(point)}";
        }

        protected abstract string GenerateProbePrefix(MethodContext ctx, CrossPoint point);
        protected abstract string GenerateProbeData(CrossPoint point);
        #endregion
        #region CrossPoint        
        /// <summary>
        /// Gets the cross-point of the Target's code by its type and localId (create if needed).
        /// </summary>
        /// <param name="ctx">The target method's context</param>
        /// <param name="pointType">Type of the cross-point.</param>
        /// <param name="localId">The local identifier of the cross-point.</param>
        /// <returns></returns>
        public virtual CrossPoint GetOrCreatePoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var point = ctx.Method.Points
                .FirstOrDefault(a => a.PointType == pointType && a.PointId == localId.ToString()); //check for PointType need to use also
            if (point != null)
                return point;
            point = CreateCrossPoint(ctx, pointType, localId);
            ctx.Method.Add(point);
            return point;
        }

        /// <summary>
        /// Creates the cross-point of the Target's code by its type and localId.
        /// </summary>
        /// <param name="ctx">The target method's context</param>
        /// <param name="pointType">Type of the cross-point.</param>
        /// <param name="localId">The local identifier of the cross-point.</param>
        /// <returns></returns>
        protected virtual CrossPoint CreateCrossPoint(MethodContext ctx, CrossPointType pointType, int localId)
        {
            var pointUid = Guid.NewGuid().ToString();
            var businessIndex = CalcBusinessIndex(ctx, localId);
            var point = new CrossPoint(pointUid, localId.ToString(), businessIndex, pointType)
            {
                //TODO: PDB data
            };
            return point;
        }

        /// <summary>
        /// Calculates the index of the cross-point's instruction in the ideal business code 
        /// (collected from the compiler generated parts of IL code) by local index of the instruction
        /// in these compiler generated methids and classes.
        /// </summary>
        /// <param name="ctx">The target method's context</param>
        /// <param name="localIndex">Local index of the cross-point.</param>
        /// <returns></returns>
        internal virtual int CalcBusinessIndex(MethodContext ctx, int localIndex)
        {
            var ind = localIndex;
            var method = ctx.Method;
            //go up to business method and sum the real index shift (business index)
            while (true)
            {
                var info = method.CGInfo;
                var caller = info?.Caller;
                if (caller == null)
                    break;
                var indexes = caller.CalleeIndexes;
                if (!indexes.ContainsKey(method.FullName))
                    break;
                ind += indexes[method.FullName];
                method = caller;
            }
            return ind;
        }
        #endregion
    }
}

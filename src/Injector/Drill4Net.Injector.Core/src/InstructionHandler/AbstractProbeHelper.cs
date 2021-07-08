using System;
using System.Linq;
using Drill4Net.Common;
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
            var businessIndex = CalcBusinessIndex(ctx.Method, ctx.GetCurBusinessIndex(localId));
            var point = new CrossPoint(pointUid, localId.ToString(), businessIndex, pointType)
            {
                //TODO: PDB data
            };
            return point;
        }

        /// <summary>
        /// Calculates the index of the cross-point's instruction in the ideal business code 
        /// (collected from the compiler generated parts of IL code) by local index of the instruction
        /// in these compiler generated methods and classes.
        /// </summary>
        /// <param name="ctx">The target method's context</param>
        /// <param name="localBusinessIndex">Local index of the cross-point.</param>
        /// <returns></returns>
        internal virtual int CalcBusinessIndex(InjectedMethod method, int localBusinessIndex)
        {
            var ind = localBusinessIndex;
            //go up to the business method and get the "business" (logical) index
            //of instruction taking into account the shift of the callee calls
            while (true)
            {
                var caller = method.CGInfo?.Caller;
                if (caller == null)
                    break;
                var indexes = caller.CalleeIndexes;
                var curName = method.FullName;
                if (!indexes.ContainsKey(curName))
                    break;
                ind += indexes[curName];
                method = caller;
            }
            return ind;
        }
        #endregion
    }
}

using System;
using System.Linq;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Helper for constructing the method block structure, for the further coverage calculations
    /// </summary>
    internal static class StructureHelper
    {
        /// <summary>
        /// The calculating of the local code block coverage (cross-point's instruction ranges)
        /// fro assembly's methods
        /// </summary>
        /// <param name="asmCtx">Assembly context</param>
        internal static void CalcMethodBlocks(AssemblyContext asmCtx)
        {
            var allMethods = asmCtx.InjMethodByFullname.Values;
            foreach (var method in allMethods)
            {
                //for the block type (!) of coverage If/Else type not needed
                var points = method.Points;
                var ranges = points
                    .Select(a => a.BusinessIndex)
                    .Where(c => c != 0) //"Enter" type not needed in any case (for the block type of coverage)
                    .OrderBy(b => b)
                    .Distinct() //needs to exclude, in fact, some excess/fictive for coverage injections: CycleEnd, etc
                    .ToList();
                if (!ranges.Any())
                    continue;
                //
                var structure = method.Structure;
                foreach (var ind in ranges)
                {
                    //some paired points may have same index (Cycle/CycleEnd)
                    var points2 = points.Where(a => a.BusinessIndex == ind).ToList();
                    if (points2.Count > 1)
                        points2 = points2.Where(a => a.PointType != CrossPointType.CycleEnd).ToList(); //Guanito...
                    structure.PointToBlockEnds.Add(points2[0].PointUid, ind);
                }
            }
        }
    }
}

﻿using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CrossPoint : InjectedSimpleEntity
    {
        public string PointUid { get; set; }
        public string PointId { get; set; }
        
        /// <summary>
        /// End-to-end index of the instruction, taking into account
        /// only the business parts of the source code moved by the compiler
        /// to the classes generated by it
        /// </summary>
        public int BusinessIndex { get; }
        public CrossPointType PointType { get; set; }

        #region PDB
        public int? RowStart { get; set; }
        public int? RowEnd { get; set; }
        public int? ColStart { get; set; }
        public int? ColEnd { get; set; }
        #endregion

        /************************************************************************/

        public CrossPoint(string pointUid, string pointId, int businessIndex, CrossPointType pointType): 
            base(null)
        {
            PointUid = pointUid;
            PointId = pointId;
            BusinessIndex = businessIndex;
            PointType = pointType;
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"P: {PointUid}: {PointType}_{PointId}";
        }
    }
}

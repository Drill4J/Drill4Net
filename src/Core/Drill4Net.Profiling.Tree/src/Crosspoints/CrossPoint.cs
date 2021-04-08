﻿using System;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CrossPoint : InjectedSimpleEntity
    {
        public string PointUid { get; set; }
        public string PointId { get; set; }
        public CrossPointType PointType { get; set; }

        #region PDB
        public int? RowStart { get; set; }
        public int? RowEnd { get; set; }
        public int? ColStart { get; set; }
        public int? ColEnd { get; set; }
        #endregion

        /************************************************************************/

        public CrossPoint(string pointUid, string pointId, CrossPointType pointType): base(null)
        {
            PointUid = pointUid;
            PointId = pointId;
            PointType = pointType;
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"P: {PointUid}: {PointType}_{PointId}";
        }
    }
}

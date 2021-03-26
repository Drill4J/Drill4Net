using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class CrossPoint : InjectedSimpleEntity
    {
        public string PointUid { get; set; }
        public object PointId { get; set; }
        public CrossPointType PointType { get; set; }

        #region PDB
        public int? RowStart { get; set; }
        public int? RowEnd { get; set; }
        public int? ColStart { get; set; }
        public int? ColEnd { get; set; }
        #endregion

        /************************************************************************/

        public CrossPoint(string pointUid, object pointId, CrossPointType pointType): base(null)
        {
            PointUid = pointUid;
            PointId = pointId;
            PointType = pointType;
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"{PointUid}/{PointId}: {PointType}";
        }
    }
}

namespace Drill4Net.Target.Common
{
    public struct CrossPoint
    {
        public string PointId { get; set; }
        public string AssemblyName { get; set; }
        public string Source { get; set; }
        public int? RowNumber { get; set; }
        public SourceType SourceType { get; set; }
        public CrossPointType PointType { get; set; }
        public object Info { get; set; }
    }
}

using System;

namespace Drill4Net.Admin.Requester
{
    [Serializable]
    public record AssociatedTestParams
    {
        public string MethodType { get; set; }
        public string Method { get; set; }
        public string MethodParams { get; set; }

        /*******************************************************/

        public override string ToString()
        {
            return $"{MethodType}: {Method}@{MethodParams}";
        }
    }
}
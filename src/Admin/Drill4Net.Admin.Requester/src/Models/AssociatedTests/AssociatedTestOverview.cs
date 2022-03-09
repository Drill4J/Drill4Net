using System;
using Drill4Net.Common;

namespace Drill4Net.Admin.Requester
{
    [Serializable]
    public record AssociatedTestOverview
    {
        public int duration { get; set; }
        public string result { get; set; }
        public AssociatedTestDetails details { get; set; }

        /********************************************************************/

        public override string ToString()
        {
            return $"{result}: {CommonUtils.ConvertFromUnixTime(duration).ToShortTimeString()}";
        }
    }
}

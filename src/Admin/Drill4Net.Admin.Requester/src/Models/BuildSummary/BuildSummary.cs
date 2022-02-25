using System;

namespace Drill4Net.Admin.Requester
{
    //https://kb.epam.com/display/EPMDJ/Builds+summary+API

    //DON'T change it by "just refactoring" because all that are DTOs for the Drill Admin's REST service

    /// <summary>
    /// Summary info about the build's coverage
    /// </summary>
    [Serializable]
    public record BuildSummary
    {
        /// <summary>
        /// In JSON can be more than one Build version name
        /// </summary>
        public string BuildVersion { get; set; }

        /// <summary>
        /// Build version arriving time to the Drill Admin. 
        /// </summary>
        public long DetectedAt { get; set; }

        /// <summary>
        /// All key metrics of the build
        /// </summary>
        public SummaryInfo Summary { get; set; }

        /*******************************************************************/

        public override string ToString()
        {
            return $"{BuildVersion} -> {Summary}";
        }
    }
}

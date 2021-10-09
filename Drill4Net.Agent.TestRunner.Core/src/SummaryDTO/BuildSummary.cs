﻿using System;

namespace Drill4Net.Agent.TestRunner.Core
{
    //https://kb.epam.com/display/EPMDJ/Builds+summary+API

    /// <summary>
    /// Summary info about the build's coverage
    /// </summary>
    [Serializable]
    public class BuildSummary
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
            return BuildVersion;
        }
    }
}

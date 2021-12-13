﻿using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Options for the TestRunner subsystem
    /// </summary>
    public class TestRunnerOptions : AbstractOptions
    {
        /// <summary>
        /// Directory for the injected tests
        /// </summary>
        public List<RunDirectoryOptions> Directories { get; set; }

        /// <summary>
        /// The parallel execution is restricted by default (if no builds still exists in Drill service)
        /// on Run level - for all specified directories
        /// </summary>
        public bool DefaultParallelRestrict { get; set; }

        /// <summary>
        /// Options exclusively for debug
        /// </summary>
        public TestRunnerDebugOptions Debug { get; set; }

        /***********************************************************************/

        public override string ToString()
        {
            return $"Directories: {Directories.Count}, DefaultParallelRestrict: {DefaultParallelRestrict}";
        }
    }
}

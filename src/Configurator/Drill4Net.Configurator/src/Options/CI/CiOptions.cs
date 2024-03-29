﻿using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Options for a full run "from alpha to omega": performing injections, test runs...
    /// </summary>
    public class CiOptions : AbstractOptions
    {
        public BatchInjectionOptions? Injection { get; set; }

        /// <summary>
        /// The path to the TestRunner config used to run tests in injected targets
        /// </summary>
        public string? TestRunnerConfigPath { get; set; }

        /*******************************************************************/

        public CiOptions()
        {
            Type = CoreConstants.SUBSYSTEM_CI;
        }
    }
}
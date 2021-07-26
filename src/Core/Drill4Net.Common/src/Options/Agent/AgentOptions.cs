﻿using System;

namespace Drill4Net.Common
{
    /// <summary>
    /// Agent's options
    /// </summary>
    [Serializable]
    public class AgentOptions : BaseTargetOptions
    {
        /// <summary>
        /// Options for the communicating between Agent part of instrumented App and the admin side
        /// </summary>
        public AdminOptions Admin { get; set; }
    }
}

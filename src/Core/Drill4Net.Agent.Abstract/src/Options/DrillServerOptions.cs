using System;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Options for communicate with Admin side of Drill system
    /// </summary>
    [Serializable]
    public class DrillServerOptions
    {
        /// <summary>
        /// Endpoint for communication (address with port)
        /// </summary>
        public string Url { get; set; }

        //TODO: cred props
        //public string Login { get; set; }
        //public string Password { get; set; }

        //endpoints, etc
    }
}

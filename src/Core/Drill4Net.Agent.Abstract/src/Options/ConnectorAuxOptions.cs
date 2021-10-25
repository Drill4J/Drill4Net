using System;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// The parameters for the connector auxiliary subsystem (native Drill library), it can be missed
    /// </summary>
    [Serializable]
    public class ConnectorAuxOptions
    {
        /// <summary>
        /// Directory for the Connector logs.
        /// If empty, will be used the current common log directory
        /// </summary>
        public string LogDir { get; set; }

        /// <summary>
        /// Name of the log file or its full path (in this case LogDir will be ignored). 
        /// It can be empty
        /// </summary>
        public string LogFile { get; set; }

        public string LogLevel { get; set; }
    }
}

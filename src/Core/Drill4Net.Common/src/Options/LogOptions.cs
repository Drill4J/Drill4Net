namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the logging
    /// </summary>
    public class LogOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LogOptions"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the type of the logger sink (file, console, etc).
        /// By default it is file type.
        /// Possible types: file, console.
        /// </summary>
        /// <value>
        /// The type of the logger sink.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the path/URI, thus, for file type it is full file path.
        /// If its extension isn't exists for th file name will be used name of the subsystem. 
        /// For console it is empty.
        /// </summary>
        /// <value>
        /// The file log path, directory, etc.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the log level (Verbose, Info, Error, etc).
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public string Level { get; set; }
    }
}

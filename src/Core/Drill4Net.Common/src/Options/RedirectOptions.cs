﻿namespace Drill4Net.Common
{
    /// <summary>
    /// Simple redirection cfg to real Injection cfg
    /// </summary>
    public class RedirectOptions
    {
        /// <summary>
        /// Full/relative path or simple name of real injection cfg (in this case with or without extension)
        /// </summary>
        public string Path { get; set; }
    }
}

namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the Destination: path, naming, etc
    /// </summary>
    public class DestinationOptions
    {
        /// <summary>
        /// Where copying injected assemblies. May be empty if no empty <see cref="FolderPostfix"/>
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// If <see cref="Directory"/> is empty, the destination path 
        /// will be construct as Source path + "." + this property
        /// </summary>
        public string FolderPostfix { get; set; }
    }
}

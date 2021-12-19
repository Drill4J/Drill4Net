using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Plugins.SpecFlow
{
    /// <summary>
    /// Specific plugin options
    /// </summary>
    public class SpecFlowPluginOptions : AbstractOptions
    {
        /// <summary>
        /// Options for the Filter of directories, folders, namespaces, type names, etc
        /// </summary>
        public SourceFilterOptions Filter { get; set; }
    }
}

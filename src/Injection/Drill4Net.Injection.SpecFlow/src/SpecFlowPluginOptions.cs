using Drill4Net.Configuration;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injection.SpecFlow
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

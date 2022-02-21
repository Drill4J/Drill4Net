using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for the Injector app itself
    /// </summary>
    public class InjectorAppOptions : AbstractOptions
    {
        /// <summary>
        /// Directory for the Injector's plugins. If it empty will be used "plugin" folder in the root of the Injector.
        /// </summary>
        public string PluginDir { get; set; }

        /******************************************************************************/

        public InjectorAppOptions()
        {
            Type = CoreConstants.SUBSYSTEM_INJECTOR_APP;
        }
    }
}

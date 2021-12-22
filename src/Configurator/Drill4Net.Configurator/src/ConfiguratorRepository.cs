using System.IO;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Configurator
{
    public class ConfiguratorRepository : AbstractRepository<ConfiguratorOptions>
    {
        public ConfiguratorRepository(ConfiguratorOptions? opts = null) : base(CoreConstants.SUBSYSTEM_CONFIGURATOR, false)
        {
            Options = opts ?? GetOptionsByPath(Subsystem);
        }

        /*******************************************************************************/

        public static ConfiguratorOptions GetOptionsByPath(string subsystem)
        {
            var optHelper = new BaseOptionsHelper<ConfiguratorOptions>(subsystem);
            var cfgPath = Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_DEFAULT);
            return optHelper.ReadOptions(cfgPath);
        }
    }
}

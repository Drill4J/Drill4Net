using System;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Configurator
{
    public class ConfiguratorRepository : AbstractRepository<ConfiguratorOptions>
    {
        public ConfiguratorRepository() : base(CoreConstants.SUBSYSTEM_CONFIGURATOR)
        {
        }
    }
}

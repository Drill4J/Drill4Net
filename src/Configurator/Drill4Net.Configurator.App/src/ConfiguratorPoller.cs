using System;
using System.Linq;

namespace Drill4Net.Configurator.App
{
    internal class ConfiguratorPoller
    {
        private readonly ConfiguratorOutputHelper _outputHelper;

        /**********************************************************************/

        public ConfiguratorPoller(ConfiguratorOutputHelper outputHelper)
        {
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        /**********************************************************************/

        public void Init()
        {
            _outputHelper.WriteMessage("Configurator is initializing", ConfiguratorAppConstants.COLOR_TEXT);
        }

        public void Start()
        {

        }
    }
}

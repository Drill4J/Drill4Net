using Drill4Net.Common;

namespace Drill4Net.Configurator.App
{
    internal class ConfiguratorInformer
    {
        private readonly ConfiguratorOutputHelper _helper;

        /*******************************************************************/

        public ConfiguratorInformer(ConfiguratorOutputHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        /*******************************************************************/

        internal string SetTitle()
        {
            var version = CommonUtils.GetAppVersion();
            var appName = CommonUtils.GetAppName();
            var title = $"{appName} {version}";
            Console.Title = title;
            _helper.WriteLine(title, ConsoleColor.Cyan);
            return title;
        }
    }
}

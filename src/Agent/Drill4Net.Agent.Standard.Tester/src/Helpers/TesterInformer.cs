using System;
using System.Reflection;
using Drill4Net.Common;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Functions for setting Tester app up
    /// </summary>
    internal class TesterInformer
    {
        private readonly OutputInfoHelper _helper;

        /*******************************************************************/

        public TesterInformer(OutputInfoHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        /*******************************************************************/

        internal void SetTitle()
        {
            var version = GetAppVersion();
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var title = $"{appName} {version}";
            Console.Title = title;
            _helper.WriteMessage(title, ConsoleColor.Cyan);
        }

        private string GetAppVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
        }
    }
}

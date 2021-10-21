using System;
using System.Reflection;
using Drill4Net.Common;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Functions for setting Tester app up
    /// </summary>
    internal static class TesterConfiguration
    {
        internal static void SetTitle()
        {
            var version = GetAppVersion();
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var title = $"{appName} {version}";
            Console.Title = title;
            OutputInfoHelper.WriteMessage(title, ConsoleColor.Cyan);
        }

        private static string GetAppVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
        }
    }
}

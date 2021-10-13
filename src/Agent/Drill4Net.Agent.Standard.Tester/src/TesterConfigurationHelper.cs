﻿using Drill4Net.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Standard.Tester
{
    internal static class TesterConfigurationHelper
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

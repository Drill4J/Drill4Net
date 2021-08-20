using System;
using System.Linq;
using System.Reflection;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Service
{
    public static class ServiceUtils
    {
        internal static string GetAppVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
        }

        internal static string GetAppName()
        {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}

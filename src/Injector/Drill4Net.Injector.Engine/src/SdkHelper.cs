using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Drill4Net.Injector.Engine
{
    public static class SdkHelper
    {
        // Search dotnet paths for reference assemblies
        // Paths based on info from https://docs.microsoft.com/en-us/dotnet/core/distribution-packaging
        private const string DotNetPath = @"c:\Program Files\dotnet\";
        private const string PacksPath = @"packs\";
        private const string CoreRefsPath = DotNetPath + PacksPath + @"Microsoft.NETCore.App.Ref\";
        private const string StandardRefsPath = DotNetPath + PacksPath + @"NETStandard.Library.Ref\";

        public static string[] GetInstalledCoreRefsVersions() =>
            GetInstalledRefsVersions(CoreRefsPath);
        public static string[] GetInstalledStandardRefsVersions() =>
            GetInstalledRefsVersions(StandardRefsPath);

        // Use dotnet tool to get sdk and runtime versions and paths
        private const string DotNetTool = "dotnet";

        /*******************************************************************************/

        private static string[] GetInstalledRefsVersions(string baseRefPath)
        {
            var dir = Directory.GetDirectories(baseRefPath);
            return Array.ConvertAll(dir, d => Path.GetRelativePath(baseRefPath, d));
        }

        public static string GetCoreAssemblyPath(string refVersion, string assemblyName) =>
            GetReferenceAssemblyPath(CoreRefsPath, refVersion, assemblyName);
        public static string GetStandardAssemblyPath(string refVersion, string assemblyName) =>
            GetReferenceAssemblyPath(StandardRefsPath, refVersion, assemblyName);

        private static string GetReferenceAssemblyPath(string baseRefPath, string refVersion, string assemblyName)
        {
            var frameworkList = Path.Combine(baseRefPath, refVersion, @"data\FrameworkList.xml");
            if (!File.Exists(frameworkList))
                return null;
            var xDocument = XDocument.Load(frameworkList);
            var (path, _) = xDocument.Descendants(@"File")
                .Select(x => (path: x.Attribute(@"Path"), name: x.Attribute("AssemblyName")))
                .FirstOrDefault(t => string.Equals(t.name.Value, assemblyName, StringComparison.InvariantCultureIgnoreCase));
            return path == null
                ? null
                : Path.Combine(baseRefPath, refVersion, path.Value);//.Replace('/', '\\');
        }

        public static string GetInstalledSdks() => RunDotNetTool(@"--list-sdks");
        public static string GetInstalledRuntimes() => RunDotNetTool(@"--list-runtimes");
        public static string GetCurrentSdkVersion() => RunDotNetTool(@"--version");

        //public static string GetSdkPath(string sdkVersion)
        //{
        //    var sdkListString = GetInstalledSdks();
        //    var sdkInfo = sdkListString.Split(Environment.NewLine).FirstOrDefault(x => x.StartsWith(sdkVersion));

        //    if (string.IsNullOrEmpty(sdkInfo))
        //        return null;

        //    var match = Regex.Match(sdkInfo, @"\[(?<path>.*)\]");
        //    var group = match.Groups.FirstOrDefault(x => x.Name == "path");
        //    if (group == null)
        //        return null;

        //    var sdkBasePath = group.Value;
        //    return Path.Combine(sdkBasePath, sdkVersion);
        //}

        //public static string GetCurrentSdkPath()
        //{
        //    var currentSdkVersion = GetCurrentSdkVersion();
        //    return string.IsNullOrEmpty(currentSdkVersion) ? null : GetSdkPath(currentSdkVersion);
        //}

        private static string RunDotNetTool(string arguments) =>
            Process.Start(new ProcessStartInfo(DotNetTool, arguments)
            {
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            })
                ?.StandardOutput.ReadToEnd()
                .Trim();
    }
}
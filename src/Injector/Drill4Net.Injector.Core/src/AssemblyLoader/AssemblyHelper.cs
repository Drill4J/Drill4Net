using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Drill4Net.Injector.Core
{
    public class AssemblyHelper
    {
        public string FindAssemblyPath(string shortName, Version version)
        {
            //root runtime path - TODO: regex
            var curPath = RuntimeEnvironment.GetRuntimeDirectory();
            var arP = curPath.Split('\\').ToList();
            for (var i = 0; i < 3; i++)
                arP.RemoveAt(arP.Count - 1);
            var runtimeRootPath = string.Join("\\", arP);

            //runtime version
            var verS = $"{version.Major}.{version.Minor}";

            //search
            if(!shortName.EndsWith(".dll"))
                shortName = $"{shortName}.dll";
            var dirs = Directory.GetDirectories(runtimeRootPath);
            foreach (var dir in dirs)
            {
                var may = $"{dir}\\{verS}";
                var verDir = Directory.GetDirectories(dir).FirstOrDefault(a => a.StartsWith(may));
                if (verDir == null)
                    continue;
                var filePath = Path.Combine(verDir, shortName);
                if (!File.Exists(filePath))
                    continue;
                return filePath;
            }
            return null;
        }
    }
}

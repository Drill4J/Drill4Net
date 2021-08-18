using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core
{
    public static class InjectorCoreUtils
    {
        public static string GetDestinationDirectory(InjectorOptions opts, string currentDir)
        {
            string destDir = FileUtils.GetFullPath(opts.Destination.Directory);
            if (!FileUtils.IsSameDirectories(currentDir, opts.Source.Directory))
            {
                var origPath = FileUtils.GetFullPath(opts.Source.Directory);
                destDir = Path.Combine(destDir, currentDir.Remove(0, origPath.Length));
            }
            return destDir;
        }
    }
}

using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Testing
{
    public class TesterRepository : AbstractRepository<TesterOptions, BaseOptionsHelper<TesterOptions>>
    {
        public TesterRepository(string cfgPath = null) : base(cfgPath)
        {
        }

        /***************************************************************************/

        public string GetTargetsDir(string callingDir)
        {
            var baseDir = GetBaseDir(callingDir);
            if (baseDir == null)
                throw new Exception($"Base directory for tests is empty. See {CoreConstants.CONFIG_TESTS_NAME}");
            //var folder = Options?.Versions?.Directory;
            if (baseDir.EndsWith("\\"))
                baseDir = baseDir.Remove(baseDir.Length - 1, 1);
            return Path.Combine(baseDir, Options.TreePath); // $"{baseDir}\\{Options.Folder}";
        }

        public string GetBaseDir(string callingDir)
        {
            return FindTestsDir(callingDir);
        }

        internal string FindTestsDir(string innerDir)
        {
            var targets = Options.Versions.Targets;
            //crunch...
            if (!targets.ContainsKey("netstandard2.0"))
                targets.Add("netstandard2.0", null);
            if (!targets.ContainsKey("netstandard2.1"))
                targets.Add("netstandard2.1", null);
            //
            var di = new DirectoryInfo(innerDir);
            if (targets.ContainsKey(di.Name))
                di = di.Parent;
            return di.Parent.FullName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Drill4Net.Common.Helpers
{
    public static class AssemblyGitInfo
    {

        /// <summary>
        /// Get Git Source Branch Name for current assembly
        /// </summary>
        /// <returns>Git Source Branch Name</returns>
        public static string GetSourceBranchName()
        {
            return ThisAssembly.Git.Branch;
        }

        /// <summary>
        /// Get Git commit for current assembly
        /// </summary>
        /// <returns>Git Commit</returns>
        public static string GetCommit()
        {
            return ThisAssembly.Git.Commit;
        }
    }
}

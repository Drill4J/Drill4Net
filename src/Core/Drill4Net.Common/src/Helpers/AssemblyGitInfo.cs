namespace Drill4Net.Common.Helpers
{
    /// <summary>
    /// Get Git Information for sources
    /// </summary>
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
        /// Get Git commit for current assembly.
        /// Works correctly for entities using it only in case of full compilation from sources.
        /// </summary>
        /// <returns>Git Commit</returns>
        public static string GetCommit()
        {
            return ThisAssembly.Git.Commit;
        }
    }
}

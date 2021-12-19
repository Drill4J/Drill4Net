namespace Drill4Net.Common
{
    /// <summary>
    /// Types of the assembly version
    /// </summary>
    public enum AssemblyVersionType
    {
        /// <summary>
        /// Version isn't set yet or isn't recognized
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Assembly not contains the IL code
        /// </summary>
        NotIL = 1,

        /// <summary>
        /// It's NetStandard version
        /// </summary>
        NetStandard = 2,

        /// <summary>
        /// It's NetFramework version
        /// </summary>
        NetFramework = 3,

        /// <summary>
        /// It's .Net Core version
        /// </summary>
        NetCore = 4,
    }
}

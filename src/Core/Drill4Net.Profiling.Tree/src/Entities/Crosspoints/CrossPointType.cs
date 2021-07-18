namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// The type of the cross-point in the intrumented Target
    /// </summary>
    public enum CrossPointType
    {
        Unset = 0,

        //common funcs
        Enter = 1,
        Return = 2,

        //branch
        If = 7,
        Else = 8,
        Switch = 9,

        //cycles
        Cycle = 10,
        CycleEnd = 11,

        //try/catch
        Throw = 12,
        CatchFilter = 13,
        FinalyLeave = 14, //?

        /// <summary>
        /// The call of the business method
        /// </summary>
        Call = 15,

        /// <summary>
        /// The anchor for the jumper instructions
        /// </summary>
        Anchor = 16,

        /// <summary>
        /// The conditional and non-conditional branches (cross-point prior the IF/ELSE, br and br.s instructions)
        /// </summary>
        Branch = 17,

        /// <summary>
        /// The virtual type when the cross-point, in fact, isn't injected into the IL code
        /// but it is needed for the injection process in general
        /// </summary>
        Virtual = 100,
    }
}

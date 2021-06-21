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

        //misc
        Call = 15,
        Jumper = 16,
        Monitor = 25,


        Anchor = 100,
    }
}

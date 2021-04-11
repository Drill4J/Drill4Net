namespace Drill4Net.Profiling.Tree
{
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
        Monitor = 16,

        Anchor = 100,
    }
}

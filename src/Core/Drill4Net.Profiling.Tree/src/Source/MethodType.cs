namespace Drill4Net.Profiling.Tree
{
    public enum MethodType
    {
        Unset = 0,
        CompilerGenerated = 1,
        Normal = 2,
        Constructor = 3,
        Destructor = 4,
        //Anonymous = 5,
        Local = 6,
        Getter = 7,
        Setter = 8,
        EventAdd = 9,
        EventRemove = 10,
    }
}